/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Outsourced worker interface
    /// </summary>
    public interface IWorker
    {
        /// <summary>
        /// Does the work asynchronously.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="workItem">The work item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result.</returns>
        Task<TResult> DoWorkAsync<TResult>(ISerializableWorkItem<TResult> workItem, CancellationToken cancellationToken);
    }
}
