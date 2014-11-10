/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Outsourced worker.
    /// Retrieves a connection from the provider, sends the work item,
    /// waits for the result, closes the connection and returns the result.
    /// </summary>
    public class OutsourcedWorker : IWorker
    {
        /// <summary>The connection prove provider</summary>
        private readonly IWorkerConnectionProvider _connectionProveProvider;

        /// <summary>A value indicating whether exceptions should be suppressed</summary>
        private readonly bool _suppressExceptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutsourcedWorker" /> class.
        /// </summary>
        /// <param name="connectionProvider">The connection provider.</param>
        /// <param name="suppressExceptions">if set to <c>true</c> [suppress exceptions].</param>
        public OutsourcedWorker(IWorkerConnectionProvider connectionProvider, bool suppressExceptions)
            : this(connectionProvider)
        {
            _suppressExceptions = suppressExceptions;
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
        /// If suppress exceptions is set to <c>true</c>, in the eventuality of 
        /// an exception the default value for TResult will be returned.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="workItem">The work item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public Task<TResult> DoWorkAsync<TResult>(
                                        ISerializableWorkItem<TResult> workItem,
                                        CancellationToken cancellationToken)
        {
            return DoWorkInternalAsync(workItem, cancellationToken);
        }

        /// <summary>
        /// Does the work asynchronously.
        /// If suppress exceptions is set to <c>true</c>, in the eventuality of 
        /// an exception the default value for TResult will be returned.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="workItem">The work item.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public async Task<TResult> DoWorkAsync<TResult>(
                                        ISerializableWorkItem<TResult> workItem, 
                                        TimeSpan timeout, 
                                        CancellationToken cancellationToken)
        {
            var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var workTask = DoWorkInternalAsync(workItem, cancellationSource.Token);
            var timeoutTask = Task.Delay(timeout, cancellationSource.Token);
            var firstToComplete = await Task.WhenAny(workTask, timeoutTask).ConfigureAwait(false);

            cancellationSource.Cancel();

            var timeoutOccurred = timeoutTask == firstToComplete;

            if (timeoutOccurred && !_suppressExceptions)
            {
                throw new TimeoutException(string.Format("DoWorkAsync timed out after {0:#,##0.000}s", timeout.TotalSeconds));
            }

            return timeoutOccurred
                    ? default(TResult)
                    : workTask.Result;
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
        private async Task<TResult> DoWorkInternalAsync<TResult>(
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
            catch
            {
                if (!_suppressExceptions)
                {
                    throw;
                }
            }

            return default(TResult);
        }
    }
}
