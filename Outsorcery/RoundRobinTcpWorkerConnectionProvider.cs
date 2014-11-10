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
    /// Round Robin TCP Worker Connection Provider
    /// Distributes work across each server sequentially.
    /// </summary>
    public class RoundRobinTcpWorkerConnectionProvider : TcpWorkerConnectionProviderBase
    {
        /// <summary>The current position</summary>
        private int _currentPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundRobinTcpWorkerConnectionProvider"/> class.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public RoundRobinTcpWorkerConnectionProvider(IPEndPoint endPoint)
            : base(endPoint)
        {
        }

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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task, the result is the connection result.
        /// </returns>
        protected override async Task<ConnectionResult> GetConnectionAsync(
                                                        IEnumerable<IPEndPoint> endPoints, 
                                                        CancellationToken cancellationToken)
        {
            var endPointList = endPoints.ToList();
            var currentAttempt = 0;
            var currentPosition = _currentPosition;

            do
            {
                if (++currentPosition >= endPointList.Count)
                {
                    currentPosition = 0;
                }

                var connection = await AttemptConnection(endPointList[currentPosition], cancellationToken).ConfigureAwait(false);

                if (connection.WorkerConnection == null)
                    continue;

                // This means that threaded attempts won't necessarily be true
                // round robin, but its an acceptable trade-off of roughly equal distribution
                _currentPosition = currentPosition;
                return connection;
            } 
            while (currentAttempt++ < endPointList.Count);

            return null;
        }
    }
}
