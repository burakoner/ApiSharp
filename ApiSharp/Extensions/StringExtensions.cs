namespace ApiSharp.Extensions;

public static class StringExtensions
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
}
