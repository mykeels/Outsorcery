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

    /// <summary>The program</summary>
    public class Program
    {
        /// <summary>The local end point for demonstration purposes</summary>
        private static readonly IPEndPoint LocalEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444);

        /// <summary>The console application entry point.</summary>
        public static void Main()
        {
            BasicUsageExampleAsync(new CancellationToken()).Wait();
            Console.ReadKey();
        }

        /// <summary>
        /// A basic usage example. 
        /// *** DON'T FORGET TO RUN THE SERVER FIRST! ***
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task
        /// </returns>
        public static async Task BasicUsageExampleAsync(CancellationToken cancellationToken)
        {
            WriteHeader("Basic Client Example");

            // First, we define the item of work which we want to outsource. 
            // The only requirements for a work item are that it implements 
            // ISerializableWorkItem<TResult> and that it is serializable.
            var workItem = new ExampleWorkItem
            {
                ExampleList = new List<string> { "Hello", "World" },
            };

            // Now we define a connection provider for all of our workers 
            // to use when they need a connection.
            // TcpWorkerConnectionProviders are reusable and thread-safe.
            var provider = new SingleTcpWorkerConnectionProvider(LocalEndPoint);

            // We then create a worker that will outsource the work for us. 
            // Outsourced workers are reusable and thread-safe.
            var worker = new OutsourcedWorker(provider);

            // In exactly the same way as we would when not outsourcing the work,
            // we await the result of the operation.
            var result = await worker.DoWorkAsync(workItem, cancellationToken);

            // Now we just use the result in the usual way!
            Console.WriteLine(
                        "Work completed by worker - int: {0}. string: {1}.",
                        result.IntegerValue,
                        result.StringValue);
        }

        /// <summary>
        /// Writes the header.
        /// </summary>
        /// <param name="header">The header.</param>
        private static void WriteHeader(string header)
        {
            Console.WriteLine("====================");
            Console.WriteLine(header);
            Console.WriteLine("====================");
        }
    }
}
