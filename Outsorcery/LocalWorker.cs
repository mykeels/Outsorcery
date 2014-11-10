/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Local worker.
    /// Performs the work in the local environment.
    /// </summary>
    public class LocalWorker : IWorker
    {
        /// <summary>
        /// Does the work asynchronously.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="workItem">The work item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Task<TResult> DoWorkAsync<TResult>(
                                        ISerializableWorkItem<TResult> workItem, 
                                        CancellationToken cancellationToken)
        {
            return workItem.DoWorkAsync(cancellationToken);
        }

        /// <summary>
        /// Does the work asynchronously.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="workItem">The work item.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result.
        /// </returns>
        /// <exception cref="System.TimeoutException">DoWorkAsync timed out</exception>
        public async Task<TResult> DoWorkAsync<TResult>(
                                        ISerializableWorkItem<TResult> workItem, 
                                        TimeSpan timeout,
                                        CancellationToken cancellationToken)
        {
            var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var workTask = DoWorkAsync(workItem, cancellationSource.Token);
            var timeoutTask = Task.Delay(timeout, cancellationSource.Token);
            var firstToComplete = await Task.WhenAny(workTask, timeoutTask).ConfigureAwait(false);

            cancellationSource.Cancel();

            var timeoutOccurred = timeoutTask == firstToComplete;

            if (timeoutOccurred)
            {
                throw new TimeoutException(string.Format("DoWorkAsync timed out after {0:#,##0.000}s", timeout.TotalSeconds));
            }

            return workTask.Result;
        }
    }
}
