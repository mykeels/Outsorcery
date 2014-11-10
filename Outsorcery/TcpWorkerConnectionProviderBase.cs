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
    /// Base class for TCP network implementations of IOutsourcedWorkerConnectionProvider
    /// </summary>
    public abstract class TcpWorkerConnectionProviderBase : IWorkerConnectionProvider
    {
        /// <summary>The endpoints</summary>
        private readonly IList<IPEndPoint> _endPoints;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpWorkerConnectionProviderBase" /> class.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        protected TcpWorkerConnectionProviderBase(IPEndPoint endPoint)
            : this(new List<IPEndPoint> { endPoint })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpWorkerConnectionProviderBase" /> class.
        /// </summary>
        /// <param name="endPoints">The end points.</param>
        protected TcpWorkerConnectionProviderBase(IEnumerable<IPEndPoint> endPoints)
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
            var selectedConnection = await GetConnectionAsync(_endPoints, cancellationToken).ConfigureAwait(false);

            if (selectedConnection == null || selectedConnection.WorkerConnection == null)
            {
                // TODO: Get the exceptions that occurred that created this situation and add them to the inner exceptions
                throw new InvalidOperationException("Unable to make a connection");
            }

            return selectedConnection.WorkerConnection;
        }

        /// <summary>
        /// Attempts the connection.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An awaitable task, the result is the connection result.</returns>
        protected static async Task<ConnectionResult> AttemptConnection(
                                                        IPEndPoint endpoint,
                                                        CancellationToken cancellationToken)
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

        /// <summary>
        /// Gets a connection from the available endpoints asynchronously.
        /// </summary>
        /// <param name="endPoints">The end points.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An awaitable task, the result is the connection result.</returns>
        protected abstract Task<ConnectionResult> GetConnectionAsync(
                                                        IEnumerable<IPEndPoint> endPoints,
                                                        CancellationToken cancellationToken);

        /// <summary>
        /// Connection Result
        /// </summary>
        protected class ConnectionResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionResult"/> class.
            /// </summary>
            /// <param name="connection">The connection.</param>
            /// <param name="benchmarkScore">The benchmark score.</param>
            public ConnectionResult(StreamWorkerConnection connection, int benchmarkScore)
            {
                WorkerConnection = connection;
                CurrentWorkloadBenchmarkScore = benchmarkScore;
            }

            /// <summary>
            /// Gets the worker connection.
            /// </summary>
            /// <value>
            /// The worker connection.
            /// </value>
            public StreamWorkerConnection WorkerConnection { get; private set; }

            /// <summary>
            /// Gets the current workload benchmark score.
            /// </summary>
            /// <value>
            /// The current workload benchmark score.
            /// </value>
            public int CurrentWorkloadBenchmarkScore { get; private set; }
        }
    }
}
