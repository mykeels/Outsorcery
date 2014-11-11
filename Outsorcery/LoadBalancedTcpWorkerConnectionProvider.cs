/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;
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
        /// <param name="endPoints">The end points.</param>
        public LoadBalancedTcpWorkerConnectionProvider(IEnumerable<IPEndPoint> endPoints)
            : base(endPoints)
        {
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
        protected override async Task<ConnectionResult> GetConnectionAsync(
                                                        IEnumerable<IPEndPoint> endPoints, 
                                                        int workCategoryId,
                                                        CancellationToken cancellationToken)
        {
            // Connect to all end points and get their benchmark figure
            var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var connectionTasks = endPoints
                                        .Select(s => AttemptConnection(s, workCategoryId, cancellationSource.Token))
                                        .ToList();
            
            // We try our best to get all connection responses but failing that will just take what we can get
            var allTasksTask = Task.WhenAll(connectionTasks);
            var timeoutTask = Task.Delay(1000, cancellationToken);

            await Task.WhenAny(allTasksTask, timeoutTask).ConfigureAwait(false);
            
            // Cancel any connection attempts that didn't finish in time
            cancellationSource.Cancel();

            // Get all connections that were successful
            var completedTasks = connectionTasks
                                    .Where(s => s.IsCompleted)
                                    .Select(s => s.Result);
            
            var openConnections = completedTasks
                                    .Where(w => w.WorkerConnection != null)
                                    .ToList();

            // No open connections, provide a result that explains why
            if (!openConnections.Any())
            {
                var exceptions = connectionTasks
                                    .Where(s => s.IsCompleted && s.Result.Exception != null)
                                    .Select(s => s.Result.Exception)
                                    .ToList();

                return exceptions.Any() 
                        ? new ConnectionResult(new AggregateException(exceptions))
                        : new ConnectionResult(new Exception(ConnectionFailedUnknownReasonMessage));
            }

            // Find the connection reporting the lowest workload
            var selectedConnection = openConnections
                                        .OrderBy(o => o.CurrentWorkloadBenchmarkScore)
                                        .FirstOrDefault();
            
            // Close all the connections we don't need
            foreach (var openConnection in openConnections.Where(w => w.WorkerConnection != selectedConnection.WorkerConnection))
            {
                openConnection.WorkerConnection.Dispose();
            }

            return selectedConnection;
        }
    }
}
