namespace ApiSharp.Models;

/// <summary>
/// Time synchronization info
/// </summary>
/// <remarks>
/// ctor
/// </remarks>
/// <param name="logger"></param>
/// <param name="recalculationInterval"></param>
/// <param name="syncTime"></param>
/// <param name="syncState"></param>
public class TimeSyncInfo(ILogger logger, bool syncTime, TimeSpan recalculationInterval, TimeSyncState syncState)
{
    /// <summary>
    /// Logger
    /// </summary>
    public ILogger Logger { get; } = logger;

    /// <summary>
    /// Should synchronize time
    /// </summary>
    public bool SyncTime { get; } = syncTime;

    /// <summary>
    /// Timestamp recalulcation interval
    /// </summary>
    public TimeSpan RecalculationInterval { get; } = recalculationInterval;

    /// <summary>
    /// Time sync state for the API client
    /// </summary>
    public TimeSyncState TimeSyncState { get; } = syncState;

    /// <summary>
    /// Set the time offset
    /// </summary>
    /// <param name="offset"></param>
    public void UpdateTimeOffset(TimeSpan offset)
    {
        TimeSyncState.LastSyncTime = DateTime.UtcNow;
        if (offset.TotalMilliseconds > 0 && offset.TotalMilliseconds < 500)
        {
            Logger.Log(LogLevel.Information, $"{TimeSyncState.ApiName} Time offset within limits, set offset to 0ms");
            TimeSyncState.TimeOffset = TimeSpan.Zero;
        }
        else
        {
            Logger.Log(LogLevel.Information, $"{TimeSyncState.ApiName} Time offset set to {Math.Round(offset.TotalMilliseconds)}ms");
            TimeSyncState.TimeOffset = offset;
        }
    }
}
