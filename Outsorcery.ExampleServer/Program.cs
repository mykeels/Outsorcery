/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery.ExampleServer
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>The program.</summary>
    public class Program
    {
        /// <summary>The console application entry point.</summary>
        public static void Main()
        {
            UsageExample(new CancellationToken()).Wait();
        }

        /// <summary>
        /// An example of the usage of the server.
        /// When implementing your own, don't forget to reference your work item library in the 
        /// server project. The server needs it to understand the client requests to do the work!
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task.
        /// </returns>
        public static Task UsageExample(CancellationToken cancellationToken)
        {
            var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444);

            var server = new TcpWorkServer(localEndPoint, new ExampleWorkExceptionHandler());

            return server.Run(cancellationToken);
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
