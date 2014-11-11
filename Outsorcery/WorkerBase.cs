/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Worker base class
    /// </summary>
    public abstract class WorkerBase : IWorker
    {
        /// <summary>
        /// The work error message
        /// </summary>
        private const string WorkErrorMessage = "An exception occurred while doing work";

        /// <summary>
        /// A value indicating whether exceptions should be suppressed
        /// </summary>
        private readonly bool _suppressExceptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerBase"/> class.
        /// </summary>
        /// <param name="suppressExceptions">if set to <c>true</c> [suppress exceptions].</param>
        protected WorkerBase(bool suppressExceptions)
        {
            _suppressExceptions = suppressExceptions;
        }

        /// <summary>
        /// Occurs when an exception causes a work operation to fail.
        /// </summary>
        public event EventHandler<WorkExceptionEventArgs> WorkException;

        /// <summary>
        /// Does the work asynchronously.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="workItem">The work item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public async Task<TResult> DoWorkAsync<TResult>(
                                    IWorkItem<TResult> workItem,
                                    CancellationToken cancellationToken)
        {
            try
            {
                return await DoWorkInternalAsync(workItem, cancellationToken);
            }
            catch (Exception ex)
            {
                OnWorkException(new WorkException(WorkErrorMessage, workItem, ex));
            }

            return default(TResult);
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
        /// <exception cref="System.TimeoutException">Timed out</exception>
        public async Task<TResult> DoWorkAsync<TResult>(
                                    IWorkItem<TResult> workItem, 
                                    TimeSpan timeout,
                                    CancellationToken cancellationToken)
        {
            try
            {
                var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                var workTask = DoWorkInternalAsync(workItem, cancellationSource.Token);
                var timeoutTask = Task.Delay(timeout, cancellationSource.Token);
                var firstToComplete = await Task.WhenAny(workTask, timeoutTask).ConfigureAwait(false);

                cancellationSource.Cancel();

                if (timeoutTask == firstToComplete)
                {
                    throw new TimeoutException(string.Format("Timed out after {0:#,##0.000}s", timeout.TotalSeconds));
                }

                return workTask.Result;
            }
            catch (Exception ex)
            {
                OnWorkException(new WorkException(WorkErrorMessage, workItem, ex));
            }

            return default(TResult);
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
        protected abstract Task<TResult> DoWorkInternalAsync<TResult>(
                                        IWorkItem<TResult> workItem,
                                        CancellationToken cancellationToken);

        /// <summary>
        /// Called when [work exception].
        /// </summary>
        /// <param name="exception">The exception.</param>
        protected void OnWorkException(WorkException exception)
        {
            if (WorkException != null)
            {
                WorkException(this, new WorkExceptionEventArgs(exception));
            }

            if (!_suppressExceptions)
            {
                throw exception;
            }
        }
    }
}
