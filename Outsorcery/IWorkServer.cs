/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Work server interface
    /// </summary>
    public interface IWorkServer
    {
        /// <summary>
        /// Gets the exception handler.
        /// </summary>
        /// <value>
        /// The exception handler.
        /// </value>
        IWorkExceptionHandler RemoteWorkExceptionHandler { get; }

        /// <summary>
        /// Runs the work server until cancellation is requested.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An awaitable task.</returns>
        Task Run(CancellationToken cancellationToken);
    }
}