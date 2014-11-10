/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Load Balanced TCP Worker Connection Provider
    /// Uses the benchmark figure provided by the servers to decide which server to assign the work item.
    /// </summary>
    public class LoadBalancedTcpWorkerConnectionProvider : TcpWorkerConnectionProviderBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancedTcpWorkerConnectionProvider"/> class.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public LoadBalancedTcpWorkerConnectionProvider(IPEndPoint endPoint)
            : base(endPoint)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancedTcpWorkerConnectionProvider"/> class.
        /// </summary>
        /// <param name="endPoints">The end points.</param>
        public LoadBalancedTcpWorkerConnectionProvider(IEnumerable<IPEndPoint> endPoints)
            : base(endPoints)
        {
        }

        /// <summary>
        /// Gets a connection from the available endpoints asynchronously.
        /// </summary>
        /// <param name="endPoints">The end points.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task, the result is the connection result.
        /// </returns>
        protected override async Task<ConnectionResult> GetConnectionAsync(
                                                        IEnumerable<IPEndPoint> endPoints, 
                                                        CancellationToken cancellationToken)
        {
            // Connect to all end points and get their benchmark figure
            var connectionTasks = endPoints
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

            return selectedConnection;
        }
    }
}
