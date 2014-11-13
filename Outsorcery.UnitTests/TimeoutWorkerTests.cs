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
    /// Timeout Worker Tests
    /// </summary>
    [TestFixture]
    public class TimeoutWorkerTests
    {
        /// <summary>
        /// The success value
        /// </summary>
        private const int SuccessValue = 100;

        /// <summary>
        /// Timeout Throws Work Exception test
        /// </summary>
        /// <returns>An awaitable task.</returns>
        [Test]
        [ExpectedException(typeof(WorkException))]
        public async Task TimeoutThrowsWorkException()
        {
            // ARRANGE
            var worker = new TimeoutWorker(new LocalWorker(), TimeSpan.FromMilliseconds(10));

            // ACT
            await worker.DoWorkAsync(new TestWorkItem(), new CancellationToken());
        }

        /// <summary>
        /// Timeout WorkException Inner Exception Is TimeoutException test
        /// </summary>
        /// <returns>An awaitable task.</returns>
        [Test]
        public async Task TimeoutWorkExceptionInnerExceptionIsTimeoutException()
        {
            // ARRANGE
            var worker = new TimeoutWorker(new LocalWorker(), TimeSpan.FromMilliseconds(10));

            // ACT
            try
            {
                await worker.DoWorkAsync(new TestWorkItem(), new CancellationToken());
            }
            catch (Exception ex)
            {
                // ASSERT
                Assert.That(ex.InnerException, Is.InstanceOf<TimeoutException>());
            }
        }

        /// <summary>
        /// No Timeout Returns Correct Result
        /// </summary>
        /// <returns>An awaitable task.</returns>
        [Test]
        public async Task NoTimeoutReturnsCorrectResult()
        {
            // ARRANGE
            var worker = new TimeoutWorker(new LocalWorker(), TimeSpan.FromMilliseconds(1000));

            // ACT
            var result = await worker.DoWorkAsync(new TestWorkItem(), new CancellationToken());
        
            // ASSERT
            Assert.That(result, Is.EqualTo(SuccessValue));
        }

        /// <summary>
        /// Test Work Item
        /// </summary>
        private class TestWorkItem : IWorkItem<int>
        {
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
            /// Does the work asynchronously.
            /// </summary>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>
            /// The result of the work.
            /// </returns>
            public async Task<int> DoWorkAsync(CancellationToken cancellationToken)
            {
                await Task.Delay(100, cancellationToken);
                return SuccessValue;
            }
        }
    }
}
