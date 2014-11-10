/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// TCP network implementation of IOutsourcedWorkerConnectionProvider.
    /// Uses the benchmark scores provided by the work servers to decide which
    /// server to assign the work.
    /// </summary>
    public class TcpWorkerConnectionProvider : IWorkerConnectionProvider
    {
        /// <summary>The endpoints</summary>
        private readonly IList<IPEndPoint> _endPoints;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpWorkerConnectionProvider" /> class.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public TcpWorkerConnectionProvider(IPEndPoint endPoint)
            : this(new List<IPEndPoint> { endPoint })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpWorkerConnectionProvider" /> class.
        /// </summary>
        /// <param name="endPoints">The end points.</param>
        public TcpWorkerConnectionProvider(IEnumerable<IPEndPoint> endPoints)
        {
            Contract.IsNotNull(endPoints);
            Contract.IsNotEmpty(endPoints);

            _endPoints = endPoints.ToList();
        }

        /// <summary>
        /// Gets a connection asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task. The result is the connection.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Unable to successfully make a connection</exception>
        public async Task<IWorkerConnection> GetConnectionAsync(CancellationToken cancellationToken)
        {
            // Connect to all end points and request their benchmark figure
            var connectionTasks = _endPoints
                                        .Select(s => AttemptConnection(s, cancellationToken))
                                        .ToList();
            
            // TODO: implement a timeout here where we fall back on connection.WhenAny() and take what we can get
            await Task.WhenAll(connectionTasks).ConfigureAwait(false);

            // Get all connections that were successful
            var openConnections = connectionTasks
                                        .Select(s => s.Result)
                                        .Where(w => w.WorkerConnection != null)
                                        .ToList();

            // Find the connection reporting the lowest workload
            var selectedConnection = openConnections
                                        .OrderBy(o => o.CurrentWorkloadBenchmarkScore)
                                        .FirstOrDefault();

            // Close all the connections we don't need
            foreach (var openConnection in openConnections.Where(w => w != selectedConnection))
            {
                openConnection.WorkerConnection.Dispose();
            }

            if (selectedConnection == null || selectedConnection.WorkerConnection == null)
            {
                throw new InvalidOperationException("Unable to successfully make a connection");
            }

            return selectedConnection.WorkerConnection;
        }

        private async Task<ConnectionResult> AttemptConnection(IPEndPoint endpoint, CancellationToken cancellationToken)
        {
            StreamWorkerConnection connection;
            var benchmark = int.MaxValue;

            try
            {
                var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(endpoint.Address, endpoint.Port).ConfigureAwait(false);

                connection = new StreamWorkerConnection(tcpClient.GetStream());
                benchmark = await connection.ReceiveIntAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                connection = null;
            }

            return new ConnectionResult(connection, benchmark);
        }

        private class ConnectionResult
        {
            public StreamWorkerConnection WorkerConnection { get; private set; }

            public int CurrentWorkloadBenchmarkScore { get; private set; }

            public ConnectionResult(StreamWorkerConnection connection, int benchmarkScore)
            {
                WorkerConnection = connection;
                CurrentWorkloadBenchmarkScore = benchmarkScore;
            }
        }
    }
}
