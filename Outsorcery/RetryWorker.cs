/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Retry worker.
    /// Wraps a worker and, in the case of an exception, retries the work.
    /// </summary>
    public class RetryWorker : WorkerBase
    {
        /// <summary>The internal worker</summary>
        private readonly IWorker _internalWorker;

        /// <summary>The number of retries</summary>
        private readonly int _numberOfRetries;

        /// <summary>The delay between retries</summary>
        private readonly TimeSpan _delayBetweenRetries;

        /// <summary>Only retry when this predicate is true</summary>
        private readonly Predicate<Exception> _retryWhen;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryWorker" /> class.
        /// </summary>
        /// <param name="internalWorker">The internal worker.</param>
        /// <param name="numberOfRetries">The number of retries to make including the first one.</param>
        public RetryWorker(
                    IWorker internalWorker,
                    int numberOfRetries)
            : this(internalWorker, numberOfRetries, TimeSpan.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryWorker" /> class.
        /// </summary>
        /// <param name="internalWorker">The internal worker.</param>
        /// <param name="numberOfRetries">The number of retries to make including the first one.</param>
        /// <param name="delayBetweenRetries">The delay between retries.</param>
        public RetryWorker(
                    IWorker internalWorker,
                    int numberOfRetries,
                    TimeSpan delayBetweenRetries)
            : this(internalWorker, numberOfRetries, null, delayBetweenRetries)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryWorker" /> class.
        /// </summary>
        /// <param name="internalWorker">The internal worker.</param>
        /// <param name="numberOfRetries">The number of retries to make including the first one.</param>
        /// <param name="retryWhen">Only retry when this predicate is true.</param>
        public RetryWorker(
                    IWorker internalWorker,
                    int numberOfRetries,
                    Predicate<Exception> retryWhen)
            : this(internalWorker, numberOfRetries, retryWhen, TimeSpan.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryWorker" /> class.
        /// </summary>
        /// <param name="internalWorker">The internal worker.</param>
        /// <param name="numberOfRetries">The number of retries to make including the first one.</param>
        /// <param name="retryWhen">Only retry when this predicate is true.</param>
        /// <param name="delayBetweenRetries">The delay between retries.</param>
        public RetryWorker(IWorker internalWorker, int numberOfRetries, Predicate<Exception> retryWhen, TimeSpan delayBetweenRetries)
        {
            Contract.IsNotNull(internalWorker);
            Contract.IsGreaterThanZero(numberOfRetries);

            _internalWorker = internalWorker;
            _numberOfRetries = numberOfRetries;
            _delayBetweenRetries = delayBetweenRetries;
            _retryWhen = retryWhen;
        }

        /// <summary>
        /// Does the work asynchronously - protected implementation.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="workItem">The work item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task, result is TResult.
        /// </returns>
        protected override async Task<TResult> DoWorkInternalAsync<TResult>(
                                                    IWorkItem<TResult> workItem, 
                                                    CancellationToken cancellationToken)
        {
            var attemptNumber = 0;
            var maximumAttempts = _numberOfRetries + 1;
            var exceptions = new List<Exception>();

            while (++attemptNumber <= maximumAttempts)
            {
                try
                {
                    return await _internalWorker.DoWorkAsync(workItem, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exceptions.Add((ex is WorkException) ? ex.InnerException : ex);

                    if (_retryWhen != null && !_retryWhen(ex))
                    {
                        break;
                    }
                }

                if (_delayBetweenRetries > TimeSpan.Zero
                    && attemptNumber <= maximumAttempts)
                {
                    await Task.Delay(_delayBetweenRetries, cancellationToken).ConfigureAwait(false);
                }
            }

            // No attempt succeeded, rethrow all the exceptions together
            var message = string.Format("RetryWorker failed all {0} attempts.", maximumAttempts);
            throw new AggregateException(message, exceptions);
        }
    }
}
