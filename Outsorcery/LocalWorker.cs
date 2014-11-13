/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Local worker.
    /// Performs the work in the local environment.
    /// </summary>
    public class LocalWorker : WorkerBase
    {
        /// <summary>
        /// Does the work asynchronously - protected implementation.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="workItem">The work item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task, result is TResult.
        /// </returns>
        protected override Task<TResult> DoWorkInternalAsync<TResult>(
                                            IWorkItem<TResult> workItem, 
                                            CancellationToken cancellationToken)
        {
            return workItem.DoWorkAsync(cancellationToken);
        }
    }
}
