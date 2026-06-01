namespace Conkist.GDK.Serialization
{
    /// <summary>
    /// Interface representing objects that can be serialized to and populated from JSON formats.
    /// </summary>
    public interface IJsonSerializable
    {
        /// <summary>
        /// Serializes the current object state to a JSON string representation.
        /// </summary>
        /// <param name="pretty">True to format JSON with indentation; false for compact JSON.</param>
        /// <returns>A string representing the serialized JSON data.</returns>
        string ToJson(bool pretty = false);

        /// <summary>
        /// Populates the current instance properties and fields from a JSON string.
        /// </summary>
        /// <param name="json">The JSON data string containing the values to assign.</param>
        void PopulateFromJson(string json);
    }
}
