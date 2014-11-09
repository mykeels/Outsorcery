/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery.ExampleClient
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using ExampleWorkItems;

    /// <summary>
    /// The program
    /// </summary>
    public class Program
    {
        /// <summary>The local end point for demonstration purposes</summary>
        private static readonly IPEndPoint LocalEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444);

        /// <summary>
        /// The console application entry point.
        /// </summary>
        public static void Main()
        {
            // An example demonstrating the simplest possible client usage.
            // Don't forget to run the server first!
            BasicUsageExampleAsync(new CancellationToken()).Wait();
            
            Console.ReadKey();
        }

        /// <summary>
        /// A basic usage example.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task
        /// </returns>
        public static async Task BasicUsageExampleAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("====================");
            Console.WriteLine("Basic Client Example");
            Console.WriteLine("====================");

            // First, we define the item of work which we want to outsource. 
            // The only requirements for a work item are that it implements 
            // ISerializableWorkItem<TResult> and that it is serializable.
            var workItem = new ExampleWorkItem
            {
                ExampleList = new List<string> { "Hello", "World", "I am an", "example." },
            };

            // Then we define a connection provider for all of our workers 
            // to use.  It is the connection provider's job to provide a
            // connection object to the Worker whenever it requires one.
            var provider = new BasicTcpWorkerConnectionProvider(LocalEndPoint);

            // We then create a worker to outsource the work for us. 
            // Workers are reusable and thread-safe.
            // They take a connection provider and use that provider
            // to a get a connection to the work server whenever 
            // DoWorkAsync is invoked.
            var worker = new OutsourcedWorker(provider);

            // In exactly the same way as we would when not outsourcing the work,
            // we await the result of the operation.
            var result = await worker.DoWorkAsync(workItem, cancellationToken).ConfigureAwait(false);

            // Now we just use the result in the usual way!
            Console.WriteLine(
                        "Work completed by worker - int: {0}. string: {1}.",
                        result.IntegerValue,
                        result.StringValue);
        }
    }
}
