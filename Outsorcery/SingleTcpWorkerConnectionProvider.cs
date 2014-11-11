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
    /// Single Host TCP Worker Connection Provider
    /// </summary>
    public class SingleTcpWorkerConnectionProvider : TcpWorkerConnectionProviderBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleTcpWorkerConnectionProvider" /> class.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public SingleTcpWorkerConnectionProvider(IPEndPoint endPoint)
            : base(new List<IPEndPoint> { endPoint })
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
        protected override Task<ConnectionResult> GetConnectionAsync(
                                                        IEnumerable<IPEndPoint> endPoints,
                                                        int workCategoryId,
                                                        CancellationToken cancellationToken)
        {
            return AttemptConnection(endPoints.First(), workCategoryId, cancellationToken);
        }
    }
}
