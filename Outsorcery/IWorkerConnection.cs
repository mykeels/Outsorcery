/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Outsourced worker connection interface
    /// </summary>
    public interface IWorkerConnection : IDisposable
    {
        /// <summary>
        /// Sends the object asynchronously.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An awaitable task.</returns>
        Task SendObjectAsync(object obj, CancellationToken cancellationToken);

        /// <summary>
        /// Receives the object asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An awaitable task, result is the received object.</returns>
        Task<object> ReceiveObjectAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Sends the integer asynchronously.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task.
        /// </returns>
        Task SendIntAsync(int value, CancellationToken cancellationToken);

        /// <summary>
        /// Receives the integer asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An awaitable task, result is the received object.</returns>
        Task<int> ReceiveIntAsync(CancellationToken cancellationToken);
    }
}
