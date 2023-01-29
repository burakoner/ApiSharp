namespace ApiSharp.Extensions;

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
        while (exception != null)
        {
            for (var i = 0; i < indent; i++) message.Append(' ');
            message.Append(exception.GetType().Name);
            message.Append(" - ");
            message.AppendLine(exception.Message);
            for (var i = 0; i < indent; i++) message.Append(' ');
            message.AppendLine(exception.StackTrace);

            indent += 2;
            exception = exception.InnerException;
        }

        return message.ToString();
    }
}
