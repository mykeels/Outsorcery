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
    /// Basic TCP network implementation of IOutsourcedWorkerConnectionProvider.
    /// Implements a very rudimentary load balancing that distributes tasks sequentially
    /// across available endpoints without taking task complexity or individual server
    /// workload into account.
    /// </summary>
    public class BasicTcpWorkerConnectionProvider : IWorkerConnectionProvider
    {
        /// <summary>The default maximum retry attempts</summary>
        private const int DefaultMaxRetryAttempts = 3;

        /// <summary>The maximum retries failed message</summary>
        private const string MaxRetriesFailedMessage =
            "GetConnectionAsync failed, despite {0} retry attempts. See inner exceptions for more details.";

        /// <summary>The endpoints</summary>
        private readonly IList<IPEndPoint> _endPoints;
        
        /// <summary>The _max retry attempts</summary>
        private readonly int _maxRetryAttempts;

        /// <summary>The current position in the endpoints list</summary>
        private int _currentPosition;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicTcpWorkerConnectionProvider"/> class.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public BasicTcpWorkerConnectionProvider(IPEndPoint endPoint)
            : this(endPoint, DefaultMaxRetryAttempts)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicTcpWorkerConnectionProvider" /> class.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <param name="maxRetryAttempts">The maximum retry attempts.</param>
        public BasicTcpWorkerConnectionProvider(IPEndPoint endPoint, int maxRetryAttempts)
            : this(new List<IPEndPoint> { endPoint }, maxRetryAttempts)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicTcpWorkerConnectionProvider" /> class.
        /// </summary>
        /// <param name="endPoints">The end points.</param>
        public BasicTcpWorkerConnectionProvider(IEnumerable<IPEndPoint> endPoints) 
            : this(endPoints, DefaultMaxRetryAttempts)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicTcpWorkerConnectionProvider" /> class.
        /// </summary>
        /// <param name="endPoints">The end points.</param>
        /// <param name="maxRetryAttempts">The maximum retry attempts.</param>
        public BasicTcpWorkerConnectionProvider(IEnumerable<IPEndPoint> endPoints, int maxRetryAttempts)
        {
            Contract.IsNotNull(endPoints);
            Contract.IsNotEmpty(endPoints);
            Contract.IsGreaterThanZero(maxRetryAttempts);

            _maxRetryAttempts = maxRetryAttempts;
            _endPoints = endPoints.ToList();
        }

        /// <summary>
        /// Gets a connection asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task. The result is the connection.
        /// </returns>
        public async Task<IWorkerConnection> GetConnectionAsync(CancellationToken cancellationToken)
        {
            IList<Exception> exceptions = null;
            
            var retryAttempts = 0;
            while (retryAttempts++ < _maxRetryAttempts)
            {
                var endPoint = _endPoints[_currentPosition];

                if (++_currentPosition >= _endPoints.Count)
                {
                    _currentPosition = 0;
                }

                try
                {
                    var tcpClient = new TcpClient();
                    await tcpClient.ConnectAsync(endPoint.Address, endPoint.Port).ConfigureAwait(false);
                    return new StreamWorkerConnection(tcpClient.GetStream());
                }
                catch (SocketException socketException)
                {
                    exceptions = exceptions ?? new List<Exception>();
                    exceptions.Add(socketException);
                }
            }
            
            throw new CommunicationException(string.Format(MaxRetriesFailedMessage, _maxRetryAttempts), exceptions);
        }
    }
}
