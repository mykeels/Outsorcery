/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery.ExampleServer
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The console application entry point.
        /// </summary>
        public static void Main()
        {
            UsageExample(new CancellationToken()).Wait();
        }

        /// <summary>
        /// An example of the usage of the server.
        /// When implementing your own, don't forget to reference your work item library
        /// in the server project.
        /// The server needs it to understand the client requests to do the work!
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An awaitable task.
        /// </returns>
        public static Task UsageExample(CancellationToken cancellationToken)
        {
            var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444);
            var server = new TcpWorkServer(localEndPoint);

            server.RemoteWorkException += 
                (s, e) => Console.WriteLine("Exception: {0} - {1}", e.Exception.Message, e.Exception.InnerException.Message);
                                    
            return server.Run(cancellationToken);
        }
    }
}
