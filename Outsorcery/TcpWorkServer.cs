/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// TCP Network Work Server
    /// </summary>
    public class TcpWorkServer : IWorkServer
    {
        /// <summary>The local endpoint</summary>
        private readonly IPEndPoint _localEndPoint;

        /// <summary>The benchmark for our current workload</summary>
        private readonly IWorkloadBenchmark _workloadBenchmark;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpWorkServer"/> class.
        /// </summary>
        /// <param name="localEndPoint">The local endpoint.</param>
        public TcpWorkServer(IPEndPoint localEndPoint)
            : this(localEndPoint, new BasicCpuWorkloadBenchmark())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpWorkServer" /> class.
        /// </summary>
        /// <param name="localEndPoint">The local endpoint.</param>
        /// <param name="workloadBenchmark">The benchmark which provides a score of our current workload.</param>
        public TcpWorkServer(IPEndPoint localEndPoint, IWorkloadBenchmark workloadBenchmark)
        {
            Contract.IsNotNull(localEndPoint);
            Contract.IsNotNull(workloadBenchmark);

            _localEndPoint = localEndPoint;
            _workloadBenchmark = workloadBenchmark;
        }

        /// <summary>
        /// Occurs when an exception causes a remote work operation to fail.
        /// </summary>
        public event EventHandler<RemoteWorkExceptionEventArgs> RemoteWorkException;

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

                clientTasks.Add(ProcessClientAsync(client, cancellationToken));

                // Prevent the list of unresolved client tasks getting unnecessarily large
                clientTasks = clientTasks.Where(w => !w.IsCompleted).ToList();
            }

            await Task.WhenAll(clientTasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Processes the client's connection and associated work item asynchronously.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An awaitable task.</returns>
        private async Task ProcessClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            using (var connection = new StreamWorkerConnection(client.GetStream()))
            {
                IPEndPoint endpoint = null;
                dynamic workItem = null;

                try
                {
                    endpoint = (IPEndPoint)client.Client.RemoteEndPoint;

                    // Get our benchmark for the provided work category
                    var workCategoryId = await connection.ReceiveIntAsync(cancellationToken).ConfigureAwait(false);
                    var benchmark = await _workloadBenchmark.GetScoreAsync(workCategoryId).ConfigureAwait(false);

                    // Send the client our benchmark, they'll use this to determine whether to use us for the work
                    await connection.SendIntAsync(benchmark, cancellationToken).ConfigureAwait(false);
                    
                    // Try to receive work from the client, if they close the connection then we'll catch the exception
                    workItem = await connection.ReceiveObjectAsync(cancellationToken).ConfigureAwait(false);
                    
                    // do the work
                    var result = await workItem.DoWorkAsync(cancellationToken).ConfigureAwait(false);
                    
                    // Send the client the result
                    await connection.SendObjectAsync(result, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // Report then swallow any exceptions to prevent client issues bringing the server down
                    if (RemoteWorkException != null)
                    {
                        var message = string.Format(
                                        "An exception occurred when processing TCP client {0}:{1}.",
                                        endpoint != null ? endpoint.Address.ToString() : "????",
                                        endpoint != null ? endpoint.Port.ToString(CultureInfo.InvariantCulture) : "????");

                        var eventArgs = new RemoteWorkExceptionEventArgs(new RemoteWorkException(message, workItem, ex));

                        RemoteWorkException(this, eventArgs);
                    }
                }
            }
        }
    }
}
