/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Outsourced worker.
    /// Retrieves an open connection from the provider, sends the work item,
    /// waits for the result, closes the connection and returns the result.
    /// </summary>
    public class OutsourcedWorker : IWorker
    {
        /// <summary>The connection prove provider</summary>
        private readonly IWorkerConnectionProvider _connectionProveProvider;

        /// <summary>A value indicating whether socket expression should be suppressed</summary>
        private readonly bool _suppressCommunicationExceptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutsourcedWorker" /> class.
        /// </summary>
        /// <param name="connectionProvider">The connection provider.</param>
        /// <param name="suppressCommunicationExceptions">if set to <c>true</c> [suppress communication exceptions].</param>
        public OutsourcedWorker(IWorkerConnectionProvider connectionProvider, bool suppressCommunicationExceptions)
            : this(connectionProvider)
        {
            _suppressCommunicationExceptions = suppressCommunicationExceptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutsourcedWorker"/> class.
        /// </summary>
        /// <param name="connectionProvider">The connection provider.</param>
        public OutsourcedWorker(IWorkerConnectionProvider connectionProvider)
        {
            _connectionProveProvider = connectionProvider;
        }

        /// <summary>
        /// Does the work asynchronously.
        /// If suppress socket exceptions is set to <c>true</c>, in the eventuality of a socket exception
        /// the default value for TResult will be returned.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="workItem">The work item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public async Task<TResult> DoWorkAsync<TResult>(
                                        ISerializableWorkItem<TResult> workItem,
                                        CancellationToken cancellationToken)
        {
            try
            {
                using (var connection = await _connectionProveProvider
                                                .GetConnectionAsync(cancellationToken)
                                                .ConfigureAwait(false))
                {
                    await connection.SendObjectAsync(workItem, cancellationToken).ConfigureAwait(false);

                    var result = await connection.ReceiveObjectAsync(cancellationToken).ConfigureAwait(false);

                    return (TResult)result;
                }
            }
            catch (Exception ex)
            {
                if (!_suppressCommunicationExceptions)
                {
                    throw;
                }

                var aggregateException = ex as AggregateException;

                var isCommunicationExceptionOnly = ex is SocketException
                                                || (aggregateException != null
                                                    && aggregateException.InnerExceptions != null
                                                    && aggregateException.InnerExceptions.All(a => a is SocketException));

                if (!isCommunicationExceptionOnly)
                {
                    throw;
                }
            }

            return default(TResult);
        }
    }
}
