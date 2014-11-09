/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Helper class for argument validation.
    /// </summary>
    internal static class Contract
    {
        /// <summary>
        /// Determines whether [is not null] [the specified object].
        /// </summary>
        /// <typeparam name="T">The Type of the object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <exception cref="System.ArgumentNullException">Value cannot be null for Type.</exception>
        public static void IsNotNull<T>(T obj) where T : class
        {
            if (obj == null)
            {
                throw new ArgumentNullException(string.Format("Value cannot be null for Type {0}", typeof(T).Name));
            }
        }

        /// <summary>
        /// Determines whether [is not empty] [the specified enumerable].
        /// </summary>
        /// <typeparam name="T">The type of the enumerable.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <exception cref="ArgumentException">Enumerable argument cannot be empty</exception>
        public static void IsNotEmpty<T>(IEnumerable<T> enumerable)
        {
            if (!enumerable.Any())
            {
                throw new ArgumentException("Enumerable argument cannot be empty");
            }
        }

        /// <summary>
        /// Determines whether [is greater than zero] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentException">Numeric argument must be greater than zero</exception>
        public static void IsGreaterThanZero(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentException("Numeric argument must be greater than zero");
            }
        }
    }
}
