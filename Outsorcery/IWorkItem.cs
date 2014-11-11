/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Work item interface. Implementations must be serializable.
    /// </summary>
    /// <typeparam name="TResult">The Type of the work item's result.</typeparam>
    public interface IWorkItem<TResult>
    {
        /// <summary>
        /// Gets the work category identifier for this work item.
        /// </summary>
        /// <value>
        /// The work category identifier.
        /// </value>
        int WorkCategoryId { get; }

        /// <summary>
        /// Does the work asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the work.</returns>
        Task<TResult> DoWorkAsync(CancellationToken cancellationToken);
    }
}
