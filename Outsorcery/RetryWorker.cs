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

        /// <summary>The number of attempts</summary>
        private readonly int _numberOfAttempts;

        /// <summary>The delay between attempts</summary>
        private readonly TimeSpan _delayBetweenAttempts;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryWorker" /> class.
        /// </summary>
        /// <param name="internalWorker">The internal worker.</param>
        /// <param name="numberOfAttempts">The number of attempts to make.</param>
        public RetryWorker(
                    IWorker internalWorker,
                    int numberOfAttempts)
            : this(internalWorker, numberOfAttempts, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryWorker" /> class.
        /// </summary>
        /// <param name="internalWorker">The internal worker.</param>
        /// <param name="numberOfAttempts">The number of attempts to make.</param>
        /// <param name="delayBetweenAttempts">The delay between attempts.</param>
        public RetryWorker(
                    IWorker internalWorker,
                    int numberOfAttempts,
                    TimeSpan delayBetweenAttempts)
            : this(internalWorker, numberOfAttempts, delayBetweenAttempts, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryWorker" /> class.
        /// </summary>
        /// <param name="internalWorker">The internal worker.</param>
        /// <param name="numberOfAttempts">The number of attempts to make.</param>
        /// <param name="suppressExceptions">if set to <c>true</c> [suppress exceptions].</param>
        public RetryWorker(
                    IWorker internalWorker,
                    int numberOfAttempts,
                    bool suppressExceptions)
            : this(internalWorker, numberOfAttempts, TimeSpan.Zero, suppressExceptions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryWorker" /> class.
        /// </summary>
        /// <param name="internalWorker">The internal worker.</param>
        /// <param name="numberOfAttempts">The number of attempts to make.</param>
        /// <param name="delayBetweenAttempts">The delay between attempts.</param>
        /// <param name="suppressExceptions">if set to <c>true</c> [suppress exceptions].</param>
        public RetryWorker(
                    IWorker internalWorker,
                    int numberOfAttempts,
                    TimeSpan delayBetweenAttempts,
                    bool suppressExceptions)
            : base(suppressExceptions)
        {
            Contract.IsNotNull(internalWorker);
            Contract.IsGreaterThanZero(numberOfAttempts);

            _internalWorker = internalWorker;
            _numberOfAttempts = numberOfAttempts;
            _delayBetweenAttempts = delayBetweenAttempts;
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
            var exceptions = new List<Exception>();

            while (++attemptNumber <= _numberOfAttempts)
            {
                try
                {
                    return await _internalWorker.DoWorkAsync(workItem, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exceptions.Add((ex is WorkException) ? ex.InnerException : ex);
                }

                if (_delayBetweenAttempts > TimeSpan.Zero
                    && attemptNumber <= _numberOfAttempts)
                {
                    await Task.Delay(_delayBetweenAttempts, cancellationToken).ConfigureAwait(false);
                }
            }

            var message = string.Format("RetryWorker failed all {0} attempts.", _numberOfAttempts);
            throw new AggregateException(message, exceptions);
        }
    }
}
