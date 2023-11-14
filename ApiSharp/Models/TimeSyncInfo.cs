namespace ApiSharp.Models;

/// <summary>
/// Time synchronization info
/// </summary>
public class TimeSyncInfo
{
    /// <summary>
    /// Logger
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Should synchronize time
    /// </summary>
    public bool SyncTime { get; }

    /// <summary>
    /// Timestamp recalulcation interval
    /// </summary>
    public TimeSpan RecalculationInterval { get; }

    /// <summary>
    /// Time sync state for the API client
    /// </summary>
    public TimeSyncState TimeSyncState { get; }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="recalculationInterval"></param>
    /// <param name="syncTime"></param>
    /// <param name="syncState"></param>
    public TimeSyncInfo(ILogger logger, bool syncTime, TimeSpan recalculationInterval, TimeSyncState syncState)
    {
        Logger = logger;
        SyncTime = syncTime;
        RecalculationInterval = recalculationInterval;
        TimeSyncState = syncState;
    }

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
