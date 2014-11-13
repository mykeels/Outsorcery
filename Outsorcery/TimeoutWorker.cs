/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Timeout worker.
    /// Wraps a worker and, in the case that timeout is reached before the work is completed,
    /// cancels the work and throws a TimeoutException.
    /// </summary>
    public class TimeoutWorker : WorkerBase
    {
        /// <summary>The internal worker</summary>
        private readonly IWorker _internalWorker;
        
        /// <summary>The delay between attempts</summary>
        private readonly TimeSpan _timeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutWorker" /> class.
        /// </summary>
        /// <param name="internalWorker">The internal worker.</param>
        /// <param name="timeout">The timeout.</param>
        public TimeoutWorker(
                    IWorker internalWorker,
                    TimeSpan timeout)
        {
            Contract.IsNotNull(internalWorker);
            Contract.IsGreaterThanZero(timeout);

            _internalWorker = internalWorker;
            _timeout = timeout;
        }

        /// <summary>
        /// Does the work asynchronously - protected implementation.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="workItem">The work item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task, result is TResult.
        /// </returns>
        protected override async Task<TResult> DoWorkInternalAsync<TResult>(
                                                    IWorkItem<TResult> workItem, 
                                                    CancellationToken cancellationToken)
        {
            var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var workTask = _internalWorker.DoWorkAsync(workItem, cancellationSource.Token);
            var timeoutTask = Task.Delay(_timeout, cancellationSource.Token);

            var firstToComplete = await Task
                                        .WhenAny(
                                            workTask,
                                            timeoutTask)
                                        .ConfigureAwait(false);

            cancellationSource.Cancel();

            if (timeoutTask == firstToComplete)
            {
                throw new TimeoutException(string.Format("Operation timed out after {0:#,##0.000}s", _timeout.TotalSeconds));
            }

            return workTask.Result;
        }
    }
}
