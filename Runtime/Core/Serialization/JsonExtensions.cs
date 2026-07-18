using Newtonsoft.Json;

namespace Conkist.GDK.Serialization
{
    /// <summary>
    /// Extension methods providing intuitive JSON serialization and deserialization capabilities using Newtonsoft.Json.
    /// </summary>
    public static class JsonExtensions
    {
        private static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None
        };

        private static readonly JsonSerializerSettings PrettySettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.Indented
        };

        /// <summary>
        /// Converts the specified object to a JSON string representation.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="pretty">True to format with indentation; false for compact formatting.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string ToJson(this object obj, bool pretty = false)
        {
            if (obj == null) return "null";
            return JsonConvert.SerializeObject(obj, pretty ? PrettySettings : DefaultSettings);
        }

        /// <summary>
        /// Restores an object of type T from its JSON string representation.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="json">The JSON data string.</param>
        /// <returns>The deserialized object instance of type T.</returns>
        public static T FromJson<T>(this string json)
        {
            if (string.IsNullOrEmpty(json)) return default;
            return JsonConvert.DeserializeObject<T>(json, DefaultSettings);
        }

        /// <summary>
        /// Populates an existing object instance with values parsed from a JSON string.
        /// </summary>
        /// <param name="obj">The target object to populate.</param>
        /// <param name="json">The JSON data string containing the values to assign.</param>
        public static void PopulateFromJson(this object obj, string json)
        {
            if (obj == null || string.IsNullOrEmpty(json)) return;
            JsonConvert.PopulateObject(json, obj, DefaultSettings);
        }
    }
}
