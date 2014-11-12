/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;

    /// <summary>
    /// Work exception event arguments
    /// </summary>
    [Serializable]
    public class WorkExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public WorkExceptionEventArgs(WorkException exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public WorkException Exception { get; private set; }
    }
}
