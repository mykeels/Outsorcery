/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;

    /// <summary>
    /// Represents errors that occur when communicating with a remote machine. 
    /// </summary>
    [Serializable]
    public class CommunicationException : Exception
    {
        /// <summary>
        /// The default message
        /// </summary>
        private const string DefaultMessage = "A communication error occurred.";

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public CommunicationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class with a default message.
        /// </summary>
        /// <param name="inner">The inner.</param>
        public CommunicationException(Exception inner)
            : base(DefaultMessage, inner)
        {
        }
    }
}
