namespace ApiSharp.Extensions;

public static class JTokenExtensions
{
    public static JToken ToJToken(this string stringData, Log log = null)
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
            log?.Write(LogLevel.Error, info);
            if (log == null) Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | {info}");
            return null;
        }
        catch (JsonSerializationException jse)
        {
            var info = $"Deserialize JsonSerializationException: {jse.Message}. Data: {stringData}";
            log?.Write(LogLevel.Error, info);
            if (log == null) Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | {info}");
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
}
