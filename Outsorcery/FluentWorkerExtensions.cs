/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;

    /// <summary>Fluent worker extensions</summary>
    public static class FluentWorkerExtensions
    {
        /// <summary>
        /// Returns a <see cref="RetryWorker"/> wrapping this worker.
        /// </summary>
        /// <param name="worker">The worker.</param>
        /// <param name="numberOfRetries">The number of retries.</param>
        /// <returns>A retry worker.</returns>
        public static IWorker WithRetries(this IWorker worker, int numberOfRetries)
        {
            return new RetryWorker(worker, numberOfRetries);
        }

        /// <summary>
        /// Returns a <see cref="RetryWorker"/> wrapping this worker.
        /// </summary>
        /// <param name="worker">The worker.</param>
        /// <param name="numberOfRetries">The number of retries.</param>
        /// <param name="delayBetweenRetries">The delay between retries.</param>
        /// <returns>A retry worker.</returns>
        public static IWorker WithRetries(this IWorker worker, int numberOfRetries, TimeSpan delayBetweenRetries)
        {
            return new RetryWorker(worker, numberOfRetries, delayBetweenRetries);
        }

        /// <summary>
        /// Returns a <see cref="TimeoutWorker"/> wrapping this worker.
        /// </summary>
        /// <param name="worker">The worker.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>A timeout worker.</returns>
        public static IWorker WithTimeout(this IWorker worker, TimeSpan timeout)
        {
            return new TimeoutWorker(worker, timeout);
        }
    }
}
