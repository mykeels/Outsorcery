/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;

    /// <summary>
    /// Remote work exception event arguments
    /// </summary>
    [Serializable]
    public class RemoteWorkExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteWorkExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public RemoteWorkExceptionEventArgs(RemoteWorkException exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public RemoteWorkException Exception { get; private set; }
    }
}
