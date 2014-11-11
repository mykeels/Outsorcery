/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;

    /// <summary>
    /// Represents errors that occur when processing work items.
    /// </summary>
    [Serializable]
    public class WorkException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="workItem">The work item.</param>
        /// <param name="inner">The inner.</param>
        public WorkException(string message, object workItem, Exception inner)
            : base(message, inner)
        {
            WorkItem = workItem;
        }

        /// <summary>
        /// Gets the associated work item.
        /// </summary>
        /// <value>
        /// The work item.
        /// </value>
        public object WorkItem { get; private set; }
    }
}
