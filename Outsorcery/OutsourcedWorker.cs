/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Outsourced worker.
    /// Retrieves a connection from the provider, sends the work item,
    /// waits for the result, closes the connection and returns the result.
    /// </summary>
    public class OutsourcedWorker : WorkerBase
    {
        /// <summary>The connection prove provider</summary>
        private readonly IWorkerConnectionProvider _connectionProveProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutsourcedWorker" /> class.
        /// </summary>
        /// <param name="connectionProvider">The connection provider.</param>
        public OutsourcedWorker(IWorkerConnectionProvider connectionProvider)
        {
            _connectionProveProvider = connectionProvider;
        }
        
        /// <summary>
        /// Internal DoWork method.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="workItem">The work item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result.
        /// </returns>
        protected override async Task<TResult> DoWorkInternalAsync<TResult>(
                                                IWorkItem<TResult> workItem,
                                                CancellationToken cancellationToken)
        {
            using (var connection = await _connectionProveProvider
                                            .GetConnectionAsync(workItem.WorkCategoryId, cancellationToken)
                                            .ConfigureAwait(false))
            {
                await connection.SendObjectAsync(workItem, cancellationToken).ConfigureAwait(false);

                var result = await connection.ReceiveObjectAsync(cancellationToken).ConfigureAwait(false);

                return (TResult)result;
            }
        }
    }
}
