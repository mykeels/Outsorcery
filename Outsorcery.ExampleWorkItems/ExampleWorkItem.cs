/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery.ExampleWorkItems
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// An example work item - Waits 2 seconds during work to emulate a real scenario
    /// The only requirements for a work item are that it implements 
    /// IWorkItem and that it is serializable.
    /// </summary>
    [Serializable]
    public class ExampleWorkItem : IWorkItem<ExampleWorkResult>
    {
        /// <summary>
        /// Gets or sets the example list.
        /// </summary>
        /// <value>
        /// The example list.
        /// </value>
        public List<string> ExampleList { get; set; }

        /// <summary>
        /// Gets the work group identifier.
        /// </summary>
        /// <value>
        /// The work group identifier.
        /// </value>
        public int WorkCategoryId
        {
            get { return 0; }
        }

        /// <summary>
        /// Does the work asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An awaitable task, the result is the result of the work performed.</returns>
        public async Task<ExampleWorkResult> DoWorkAsync(System.Threading.CancellationToken cancellationToken)
        {
            Console.WriteLine("Work started on this system");

            await Task.Delay(2000, cancellationToken).ConfigureAwait(false);

            var result = new ExampleWorkResult
            {
                IntegerValue = new Random().Next(1000),
                StringValue = string.Join(" ", ExampleList).ToUpper()
            };

            Console.WriteLine(
                        "Work complete on this system, result - int: {0}. string: {1}.", 
                        result.IntegerValue, 
                        result.StringValue);

            return result;
        }
    }
}
