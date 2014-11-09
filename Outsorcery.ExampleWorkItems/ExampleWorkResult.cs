/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery.ExampleWorkItems
{
    using System;

    /// <summary>
    /// An example work result
    /// </summary>
    [Serializable]
    public class ExampleWorkResult
    {
        /// <summary>
        /// Gets or sets the string value.
        /// </summary>
        /// <value>
        /// The string value.
        /// </value>
        public string StringValue { get; set; }

        /// <summary>
        /// Gets or sets the integer value.
        /// </summary>
        /// <value>
        /// The integer value.
        /// </value>
        public int IntegerValue { get; set; }
    }
}
