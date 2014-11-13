/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery.ExampleClient
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using ExampleWorkItems;

    /// <summary>The program</summary>
    public class Program
    {
        /// <summary>The console application entry point.</summary>
        public static void Main()
        {
            // *** REMEMBER TO RUN THE SERVER FIRST! ***
            // *** REMEMBER TO RUN THE SERVER FIRST! ***
            // *** REMEMBER TO RUN THE SERVER FIRST! ***
            // ============================
            // SETUP
            // ============================
            // We define a connection provider for all of our workers to use when they need a connection.
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444);
            var provider = new SingleTcpWorkerConnectionProvider(remoteEndPoint);

            // We create a worker that will do the outsourcing for us. Workers are thread safe so can be shared.
            // The second argument is an implementation of IWorkExceptionHandler that allows us to specify how
            // the worker responds to exceptions.
            var worker = new OutsourcedWorker(provider, new ExampleWorkExceptionHandler());
            
            // ============================
            // WORK
            // ============================
            SimpleWorkExampleAsync(worker, new CancellationToken()).Wait();
            ConcurrentWorkExampleAsync(worker, new CancellationToken()).Wait();

            // ============================
            // FINISHED
            // ============================
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// A simple work example.
        /// </summary>
        /// <param name="worker">The worker.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task
        /// </returns>
        public static async Task SimpleWorkExampleAsync(
                                    IWorker worker,
                                    CancellationToken cancellationToken)
        {
            // We instantiate the item of work which we want to outsource. 
            var workItem = new ExampleWorkItem
            {
                ExampleList = new List<string> 
                { 
                    "Simple",
                    "example" 
                },
            };

            // await the result
            var result = await worker.DoWorkAsync(workItem, cancellationToken);

            // Now we use the result in the usual way.
            if (result != null)
            {
                Console.WriteLine(
                    "Work completed by worker - int: {0}. string: {1}.",
                    result.IntegerValue,
                    result.StringValue);
            }
        }

        /// <summary>
        /// A concurrent work example.
        /// </summary>
        /// <param name="worker">The worker.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task
        /// </returns>
        public static async Task ConcurrentWorkExampleAsync(
                                    IWorker worker, 
                                    CancellationToken cancellationToken)
        {
            var tasks = new List<Task<ExampleWorkResult>>();
            for (var i = 0; i < 20; ++i)
            {
                // We instantiate the item of work which we want to outsource. 
                var workItem = new ExampleWorkItem
                {
                    ExampleList = new List<string> 
                    { 
                        "Concurrent",
                        "example",
                        "number",
                        (i + 1).ToString(CultureInfo.InvariantCulture)
                    },
                };

                // Add the task to a list so that we can await them all at once.
                tasks.Add(worker.DoWorkAsync(workItem, cancellationToken));
            }

            // Wait for all our results to come back then output the results to console.
            await Task.WhenAll(tasks);

            foreach (var task in tasks.Where(task => task.Result != null))
            {
                Console.WriteLine(
                    "Work completed by worker - int: {0}. string: {1}.",
                    task.Result.IntegerValue,
                    task.Result.StringValue);
            }
        }

        /// <summary>
        /// An example Work Exception Handler
        /// </summary>
        private class ExampleWorkExceptionHandler : IWorkExceptionHandler
        {
            /// <summary>
            /// Handles the work exception.
            /// </summary>
            /// <param name="exception">The exception.</param>
            /// <returns>
            /// A value indicating whether to suppress the exception.
            /// </returns>
            public HandleWorkExceptionResult HandleWorkException(WorkException exception)
            {
                Console.WriteLine(
                            "Exception: {0} - {1}",
                            exception.Message,
                            exception.InnerException.Message);

                return new HandleWorkExceptionResult { SuppressException = true };
            }
        }
    }
}
