/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
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
        public Task<TResult> DoWorkAsync<TResult>(ISerializableWorkItem<TResult> workItem, System.Threading.CancellationToken cancellationToken)
        {
            return workItem.DoWorkAsync(cancellationToken);
        }
    }
}
