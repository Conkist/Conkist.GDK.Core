using System;

namespace Conkist.GDK.Serialization
{
    /// <summary>
    /// A generic abstract base class that provides automatic JSON self-serialization
    /// and static deserialization capabilities for derived data models.
    /// </summary>
    /// <typeparam name="T">The type of the derived class.</typeparam>
    [Serializable]
    public abstract class JsonSerializableBase<T> : IJsonSerializable where T : class
    {
        /// <summary>
        /// Serializes the current object state to a JSON string representation.
        /// </summary>
        /// <param name="pretty">True to format JSON with indentation; false for compact JSON.</param>
        /// <returns>A string representing the serialized JSON data.</returns>
        public virtual string ToJson(bool pretty = false)
        {
            return JsonExtensions.ToJson(this, pretty);
        }

        /// <summary>
        /// Populates the current instance properties and fields from a JSON string.
        /// </summary>
        /// <param name="json">The JSON data string containing the values to assign.</param>
        public virtual void PopulateFromJson(string json)
        {
            JsonExtensions.PopulateFromJson(this, json);
        }

        /// <summary>
        /// Deserializes a JSON string into a new instance of the derived type T.
        /// </summary>
        /// <param name="json">The JSON string containing the serialized data.</param>
        /// <returns>A new instance of type T populated with values from the JSON string.</returns>
        public static T FromJson(string json)
        {
            return JsonExtensions.FromJson<T>(json);
        }
    }
}
