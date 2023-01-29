namespace ApiSharp.Logging;

/// <summary>
/// Default log writer, uses Trace.WriteLine
/// </summary>
public class DebugLogger: ILogger
{
    public IDisposable BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var logMessage = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | {logLevel} | {formatter(state, exception)}";
        Trace.WriteLine(logMessage);
    }
}
