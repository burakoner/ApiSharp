namespace ApiSharp.Converters;

/// <summary>
/// SafeCollectionConverter is a JsonConverter that converts JSON arrays to collections of a specified type. It uses the JToken.ToObjectCollectionSafe method to perform the conversion, which ensures that the collection is populated safely and correctly.
/// </summary>
public class SafeCollectionConverter : JsonConverter
{
    /// <summary>
    /// Can convert any type. This is used to convert JSON arrays to collections of a specified type.
    /// </summary>
    /// <param name="objectType"></param>
    /// <returns></returns>
    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    /// <summary>
    /// Reads the JSON representation of the object. This method is used to convert JSON arrays to collections of a specified type.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        // Ensure the reader is not null and has content
        if (reader == null || reader.TokenType == JsonToken.None || reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        // Deserialize the JSON into a JToken
        var jToken = serializer.Deserialize<JToken>(reader) ?? throw new JsonSerializationException("Failed to deserialize JSON into a JToken.");

        // Safely convert the JToken to the desired collection type
        return jToken.ToObjectCollectionSafe(objectType, serializer);
    }

    /// <summary>
    /// CanWrite is set to false because this converter does not support writing JSON. It is only used for reading JSON arrays and converting them to collections of a specified type.
    /// </summary>
    public override bool CanWrite => false;

    /// <summary>
    /// Writes the JSON representation of the object. This method is not implemented because this converter does not support writing JSON.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    /// <exception cref="NotImplementedException"></exception>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}