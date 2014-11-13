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
        /// <summary>The work error message</summary>
        private const string WorkErrorMessage = "An exception occurred while doing work";
        
        /// <summary>
        /// Occurs when [work exception].
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
                var workException = ex as WorkException ?? new WorkException(WorkErrorMessage, workItem, ex);
                OnWorkException(workException);
                throw workException;
            }
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
        protected virtual void OnWorkException(WorkException exception)
        {
            if (WorkException != null)
            {
                WorkException(this, new WorkExceptionEventArgs(exception));
            }
        }
    }
}
