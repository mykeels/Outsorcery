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
    /// Round Robin TCP Worker Connection Provider
    /// Distributes each work item to a different endpoint, returning to the first
    /// endpoint when all endpoints have been visited.
    /// </summary>
    public class RoundRobinTcpWorkerConnectionProvider : TcpWorkerConnectionProviderBase
    {
        /// <summary>The current position</summary>
        private int _currentPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundRobinTcpWorkerConnectionProvider"/> class.
        /// </summary>
        /// <param name="endPoints">The end points.</param>
        public RoundRobinTcpWorkerConnectionProvider(IEnumerable<IPEndPoint> endPoints)
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
            var endPointList = endPoints.ToList();
            var currentAttempt = 0;
            var currentPosition = _currentPosition;
            var exceptions = new List<Exception>();

            do
            {
                if (++currentPosition >= endPointList.Count)
                {
                    currentPosition = 0;
                }

                var connection = await AttemptConnection(endPointList[currentPosition], workCategoryId, cancellationToken).ConfigureAwait(false);

                if (connection.WorkerConnection == null)
                {
                    exceptions.Add(connection.Exception ?? new Exception(ConnectionFailedUnknownReasonMessage));
                    continue;
                }

                _currentPosition = currentPosition;
                return connection;
            } 
            while (currentAttempt++ < endPointList.Count);

            // Completely failed to get a connection
            return new ConnectionResult(new AggregateException(exceptions));
        }
    }
}
