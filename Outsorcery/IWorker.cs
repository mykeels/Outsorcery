/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Worker interface
    /// </summary>
    public interface IWorker
    {
        /// <summary>
        /// Gets the exception handler.
        /// </summary>
        /// <value>
        /// The exception handler.
        /// </value>
        IWorkExceptionHandler WorkExceptionHandler { get; }

        /// <summary>
        /// Does the work asynchronously.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="workItem">The work item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result.</returns>
        Task<TResult> DoWorkAsync<TResult>(
            IWorkItem<TResult> workItem, 
            CancellationToken cancellationToken);
    }
}
