/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Outsourced worker connection provider interface
    /// </summary>
    public interface IWorkerConnectionProvider
    {
        /// <summary>
        /// Gets a connection asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An awaitable task. The result is the connection.</returns>
        Task<IWorkerConnection> GetConnectionAsync(CancellationToken cancellationToken);
    }
}
