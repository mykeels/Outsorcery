/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery.UnitTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;

    /// <summary>
    /// Retry Worker Tests
    /// </summary>
    [TestFixture]
    public class RetryWorkerTests
    {
        /// <summary>The maximum retries</summary>
        private const int MaxRetries = 2;

        /// <summary>The success value</summary>
        private const int SuccessValue = 100;

        /// <summary>
        /// Retry tries once and returns result when no exception test
        /// </summary>
        /// <returns>An awaitable task.</returns>
        [Test]
        public async Task AttemptsOnceAndReturnsCorrectResultWhenNoException()
        {
            // ARRANGE
            var worker = new RetryWorker(new LocalWorker(), MaxRetries);
            var workItem = new TestWorkItem(false);

            // ACT
            var result = await worker.DoWorkAsync(workItem, new CancellationToken());
           
            // ASSERT
            Assert.That(result, Is.EqualTo(SuccessValue));
            Assert.That(workItem.AttemptCount, Is.EqualTo(1));
        }

        /// <summary>
        /// Retry throws work exception after maximum attempts test
        /// </summary>
        /// <returns>An awaitable task.</returns>
        [Test]
        [ExpectedException(typeof(WorkException))]
        public async Task ThrowsWorkExceptionAfterMaximumAttempts()
        {
            // ARRANGE
            var worker = new RetryWorker(new LocalWorker(), MaxRetries);
            var workItem = new TestWorkItem(true);

            // ACT
            try
            {
                await worker.DoWorkAsync(workItem, new CancellationToken());
            }
            catch (Exception)
            {
                // ASSERT
                Assert.That(workItem.AttemptCount, Is.EqualTo(MaxRetries + 1));
                throw;
            }
        }

        /// <summary>
        /// Retry WorkException Inner Exception Is Aggregate Exception test
        /// </summary>
        /// <returns>An awaitable task.</returns>
        [Test]
        public async Task WorkExceptionInnerExceptionIsAggregateException()
        {
            // ARRANGE
            var worker = new RetryWorker(new LocalWorker(), MaxRetries);
            var workItem = new TestWorkItem(true);

            try
            {
                // ACT
                await worker.DoWorkAsync(workItem, new CancellationToken());
            }
            catch (Exception ex)
            {
                // ASSERT
                Assert.That(ex.InnerException, Is.TypeOf<AggregateException>());
            }
        }

        /// <summary>
        /// Retry WorkException Aggregate Exception Count Matches Number of Attempts
        /// </summary>
        /// <returns>An awaitable task.</returns>
        [Test]
        public async Task RetryWorkExceptionInnerAggregateExceptionCountMatchesNumberOfAttempts()
        {
            // ARRANGE
            var worker = new RetryWorker(new LocalWorker(), MaxRetries);
            var workItem = new TestWorkItem(true);

            try
            {
                // ACT
                await worker.DoWorkAsync(workItem, new CancellationToken());
            }
            catch (Exception ex)
            {
                // ASSERT
                Assert.That(
                    ((AggregateException) ex.InnerException).InnerExceptions.Count, 
                    Is.EqualTo(MaxRetries + 1));
            }
        }

        /// <summary>
        /// Test Work Item
        /// </summary>
        private class TestWorkItem : IWorkItem<int>
        {
            /// <summary>throw exceptions</summary>
            private readonly bool _throwExceptions;
            
            /// <summary>
            /// Initializes a new instance of the <see cref="TestWorkItem"/> class.
            /// </summary>
            /// <param name="throwExceptions"></param>
            public TestWorkItem(bool throwExceptions)
            {
                _throwExceptions = throwExceptions;
            }

            /// <summary>
            /// Gets the work category identifier for this work item.
            /// </summary>
            /// <value>
            /// The work category identifier.
            /// </value>
            public int WorkCategoryId
            {
                get { return 0; }
            }

            /// <summary>
            /// Gets the attempt count.
            /// </summary>
            /// <value>
            /// The attempt count.
            /// </value>
            public int AttemptCount { get; private set; }
            
            /// <summary>
            /// Does the work asynchronously.
            /// </summary>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>
            /// The result of the work.
            /// </returns>
            public Task<int> DoWorkAsync(CancellationToken cancellationToken)
            {
                ++AttemptCount;

                if (_throwExceptions)
                {
                    throw new InvalidOperationException();
                }

                return Task.FromResult(SuccessValue);
            }
        }
    }
}
