namespace ApiSharp.Extensions;

public static class JTokenExtensions
{
    public static JToken ToJToken(this string stringData, ILogger logger = null)
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

    public static TJToken RemoveFromLowestPossibleParent<TJToken>(this TJToken node) where TJToken : JToken
    {
        if (node == null)
            return null;

        var contained = node.AncestorsAndSelf().Where(t => t.Parent is JContainer && t.Parent.Type != JTokenType.Property).FirstOrDefault();
        if (contained != null)
            contained.Remove();

        // Also detach the node from its immediate containing property -- Remove() does not do this even though it seems like it should
        if (node.Parent is JProperty property)
            property.Value = null;

        return node;
    }

    public static IList<JToken> AsList(this IList<JToken> container) { return container; }

    public static object ToObjectCollectionSafe(this JToken jToken, Type objectType)
    {
        return ToObjectCollectionSafe(jToken, objectType, JsonSerializer.CreateDefault());
    }

    public static object ToObjectCollectionSafe(this JToken jToken, Type objectType, JsonSerializer jsonSerializer)
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
                    return jArray.First.ToObject(objectType, jsonSerializer);
            }
        }
        else if (expectArray)
        {
            //to object via JArray
            return new JArray(jToken).ToObject(objectType, jsonSerializer);
        }

        return jToken.ToObject(objectType, jsonSerializer);
    }

    public static T ToObjectCollectionSafe<T>(this JToken jToken)
    {
        return (T)ToObjectCollectionSafe(jToken, typeof(T));
    }

    public static T ToObjectCollectionSafe<T>(this JToken jToken, JsonSerializer jsonSerializer)
    {
        return (T)ToObjectCollectionSafe(jToken, typeof(T), jsonSerializer);
    }
}
