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
        /// Initializes a new instance of the <see cref="WorkerBase"/> class.
        /// </summary>
        /// <param name="exceptionHandler">The exception handler.</param>
        protected WorkerBase(IWorkExceptionHandler exceptionHandler)
        {
            WorkExceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerBase"/> class.
        /// </summary>
        protected WorkerBase()
        {
        }

        /// <summary>
        /// Gets the exception handler.
        /// </summary>
        /// <value>
        /// The exception handler.
        /// </value>
        public IWorkExceptionHandler WorkExceptionHandler { get; private set; }

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

                if (WorkExceptionHandler == null)
                {
                    throw workException;
                }

                var result = WorkExceptionHandler.HandleWorkException(workException);
                if (!result.SuppressException)
                {
                    throw workException;
                }
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
    }
}
