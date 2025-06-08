namespace ApiSharp.Extensions;

/// <summary>
/// JToken extensions for parsing and manipulating JSON data
/// </summary>
public static class JTokenExtensions
{
    /// <summary>
    /// Try to parse a string into a JToken.
    /// </summary>
    /// <param name="stringData"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static JToken? ToJToken(this string stringData, ILogger? logger = null)
    {
        if (string.IsNullOrEmpty(stringData))
            return null;

        try
        {
            return JToken.Parse(stringData);
        }
        catch (JsonReaderException jre)
        {
            var info = $"Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}. Data: {stringData}";
            logger?.Log(LogLevel.Error, info);
            if (logger == null) Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | {info}");
            return null;
        }
        catch (JsonSerializationException jse)
        {
            var info = $"Deserialize JsonSerializationException: {jse.Message}. Data: {stringData}";
            logger?.Log(LogLevel.Error, info);
            if (logger == null) Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | {info}");
            return null;
        }
    }

    /// <summary>
    /// Remove a JToken from its lowest possible parent, which is the first ancestor that has a parent that is not a JProperty.
    /// </summary>
    /// <typeparam name="TJToken"></typeparam>
    /// <param name="node"></param>
    /// <returns></returns>
    public static TJToken? RemoveFromLowestPossibleParent<TJToken>(this TJToken node) where TJToken : JToken
    {
        if (node == null)
            return null;

        var contained = node.AncestorsAndSelf().Where(t => t.Parent is not null && t.Parent.Type != JTokenType.Property).FirstOrDefault();
        contained?.Remove();

        // Also detach the node from its immediate containing property -- Remove() does not do this even though it seems like it should
        if (node.Parent is JProperty property)
            property.Value = null;

        return node;
    }

    /// <summary>
    /// AsList extension method for JToken to return the JToken as an IList of JToken.
    /// </summary>
    /// <param name="container"></param>
    /// <returns></returns>
    public static IList<JToken> AsList(this IList<JToken> container) { return container; }

    /// <summary>
    /// Convert a JToken to an object of the specified type, handling collections and single objects safely.
    /// </summary>
    /// <param name="jToken"></param>
    /// <param name="objectType"></param>
    /// <returns></returns>
    public static object? ToObjectCollectionSafe(this JToken jToken, Type objectType)
    {
        return ToObjectCollectionSafe(jToken, objectType, JsonSerializer.CreateDefault());
    }

    /// <summary>
    /// Convert a JToken to an object of the specified type, handling collections and single objects safely.
    /// </summary>
    /// <param name="jToken"></param>
    /// <param name="objectType"></param>
    /// <param name="jsonSerializer"></param>
    /// <returns></returns>
    public static object? ToObjectCollectionSafe(this JToken jToken, Type objectType, JsonSerializer jsonSerializer)
    {
        var expectArray = typeof(IEnumerable).IsAssignableFrom(objectType);

        if (jToken is JArray jArray)
        {
            if (!expectArray)
            {
                //to object via singel
                if (jArray.Count == 0)
                    return JValue.CreateNull().ToObject(objectType, jsonSerializer);

                if (jArray.Count == 1)
                    return jArray?.First?.ToObject(objectType, jsonSerializer);
            }
        }
        else if (expectArray)
        {
            //to object via JArray
            return new JArray(jToken).ToObject(objectType, jsonSerializer);
        }

        return jToken.ToObject(objectType, jsonSerializer);
    }

    /// <summary>
    /// Convert a JToken to an object of the specified type, handling collections and single objects safely.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="jToken"></param>
    /// <returns></returns>
    public static T? ToObjectCollectionSafe<T>(this JToken jToken)
    {
        return (T?)ToObjectCollectionSafe(jToken, typeof(T));
    }

    /// <summary>
    /// Convert a JToken to an object of the specified type, handling collections and single objects safely.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="jToken"></param>
    /// <param name="jsonSerializer"></param>
    /// <returns></returns>
    public static T? ToObjectCollectionSafe<T>(this JToken jToken, JsonSerializer jsonSerializer)
    {
        return (T?)ToObjectCollectionSafe(jToken, typeof(T), jsonSerializer);
    }
}
