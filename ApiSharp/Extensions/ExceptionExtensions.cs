namespace ApiSharp.Extensions;

/// <summary>
/// Extensions for Exception handling
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Format an exception and inner exception to a readable string
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public static string ToLogString(this Exception exception)
    {
        var message = new StringBuilder();
        var indent = 0;
        Exception? current = exception;
        while (current != null)
        {
            for (var i = 0; i < indent; i++) message.Append(' ');
            message.Append(current.GetType().Name);
            message.Append(" - ");
            message.AppendLine(current.Message);
            for (var i = 0; i < indent; i++) message.Append(' ');
            message.AppendLine(current.StackTrace);

            indent += 2;
            current = current.InnerException;
        }

        return message.ToString();
    }
}
