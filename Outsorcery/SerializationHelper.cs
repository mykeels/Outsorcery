/*
    License: http://www.apache.org/licenses/LICENSE-2.0
 */
namespace Outsorcery
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Helper class for data serialization
    /// </summary>
    internal static class SerializationHelper
    {
        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The serialized object as an array of bytes.</returns>
        public static byte[] SerializeObject(object obj)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The deserialized object.</returns>
        public static object DeserializeObject(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(stream);
            }
        }
    }
}
