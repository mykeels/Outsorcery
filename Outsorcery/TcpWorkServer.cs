/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// TCP Network Work Server
    /// </summary>
    public class TcpWorkServer
    {
        /// <summary>The local endpoint</summary>
        private readonly IPEndPoint _localEndPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpWorkServer"/> class.
        /// </summary>
        /// <param name="localEndPoint">The local endpoint.</param>
        public TcpWorkServer(IPEndPoint localEndPoint)
        {
            Contract.IsNotNull(localEndPoint);

            _localEndPoint = localEndPoint;
        }

        /// <summary>
        /// Runs the work server until cancellation is requested.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An awaitable task.</returns>
        public async Task Run(CancellationToken cancellationToken)
        {
            var clientTasks = new List<Task>();
            var listener = new TcpListener(_localEndPoint.Address, _localEndPoint.Port);
            
            listener.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                // The below is a shim for the lack of a cancellation token on AcceptTcpClientAsync.
                // It'll leave the Accept active, but at least it provides graceful shutdown.
                var client = await Task.Run(() => listener.AcceptTcpClientAsync(), cancellationToken)
                                        .ConfigureAwait(false);

                clientTasks.Add(HandleClientAsync(client, cancellationToken));

                // Prevent the list of unresolved client tasks getting unnecessarily large
                clientTasks = clientTasks.Where(w => !w.IsCompleted).ToList();
            }

            await Task.WhenAll(clientTasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the client's work item asynchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An awaitable task.</returns>
        private static async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            using (var connection = new StreamWorkerConnection(client.GetStream()))
            {
                try
                {
                    // SL: Not overly keen on using dynamic here, but it seems to be the only way we can
                    //     cleanly support any TResult we want in 
                    dynamic workItem = await connection.ReceiveObjectAsync(cancellationToken).ConfigureAwait(false);

                    var result = await workItem.DoWorkAsync(cancellationToken).ConfigureAwait(false);

                    await connection.SendObjectAsync(result, cancellationToken).ConfigureAwait(false);
                }
                catch (SocketException)
                {
                    // If anything bad happens to the connection just fail quietly.
                }
            }
        }
    }
}
