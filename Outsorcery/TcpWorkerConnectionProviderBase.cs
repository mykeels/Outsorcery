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
        /// <param name="workCategoryId">The work category identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task. The result is the connection.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Unable to successfully make a connection</exception>
        public async Task<IWorkerConnection> GetConnectionAsync(int workCategoryId, CancellationToken cancellationToken)
        {
            var selectedConnection = await GetConnectionAsync(_endPoints, workCategoryId, cancellationToken).ConfigureAwait(false);

            if (selectedConnection.WorkerConnection == null)
            {
                throw new InvalidOperationException("Unable to make a connection", selectedConnection.Exception);
            }

            return selectedConnection.WorkerConnection;
        }

        /// <summary>
        /// Attempts the connection.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="workCategoryId">The work category identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task, the result is the connection result.
        /// </returns>
        protected static async Task<ConnectionResult> AttemptConnection(
                                                        IPEndPoint endpoint, 
                                                        int workCategoryId,
                                                        CancellationToken cancellationToken)
        {
            StreamWorkerConnection connection;
            var benchmark = int.MaxValue;
            Exception exception = null;

            try
            {
                var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(endpoint.Address, endpoint.Port).ConfigureAwait(false);

                connection = new StreamWorkerConnection(tcpClient.GetStream());

                await connection.SendIntAsync(workCategoryId, cancellationToken).ConfigureAwait(false);
                benchmark = await connection.ReceiveIntAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                connection = null;
                exception = ex;
            }

            return new ConnectionResult(connection, benchmark, exception);
        }

        /// <summary>
        /// Gets a connection from the available endpoints asynchronously.
        /// </summary>
        /// <param name="endPoints">The end points.</param>
        /// <param name="workCategoryId">The work category identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task, the result is the connection result.
        /// </returns>
        protected abstract Task<ConnectionResult> GetConnectionAsync(
                                                        IEnumerable<IPEndPoint> endPoints,
                                                        int workCategoryId,
                                                        CancellationToken cancellationToken);

        /// <summary>
        /// Connection Result
        /// </summary>
        protected struct ConnectionResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionResult"/> struct.
            /// </summary>
            /// <param name="connection">The connection.</param>
            /// <param name="benchmarkScore">The benchmark score.</param>
            /// <param name="exception">The exception.</param>
            public ConnectionResult(StreamWorkerConnection connection, int benchmarkScore, Exception exception)
                : this()
            {
                WorkerConnection = connection;
                CurrentWorkloadBenchmarkScore = benchmarkScore;
                Exception = exception;
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

            /// <summary>
            /// Gets the exception.
            /// </summary>
            /// <value>
            /// The exception.
            /// </value>
            public Exception Exception { get; private set; }

            /// <summary>
            /// Creates from the exception.
            /// </summary>
            /// <param name="exception">The exception.</param>
            /// <returns>A connection result.</returns>
            public static ConnectionResult FromException(Exception exception)
            {
                return new ConnectionResult(null, int.MaxValue, exception);
            }
        }
    }
}
