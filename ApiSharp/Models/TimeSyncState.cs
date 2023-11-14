namespace ApiSharp.Models;

/// <summary>
/// The time synchronization state of an API client
/// </summary>
public class TimeSyncState
{
    /// <summary>
    /// Name of the API
    /// </summary>
    public string ApiName { get; set; }

    /// <summary>
    /// Semaphore to use for checking the time syncing. Should be shared instance among the API client
    /// </summary>
    public SemaphoreSlim Semaphore { get; }

    /// <summary>
    /// Last sync time for the API client
    /// </summary>
    public DateTime LastSyncTime { get; set; }

    /// <summary>
    /// Time offset for the API client
    /// </summary>
    public TimeSpan TimeOffset { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    public TimeSyncState(string apiName)
    {
        ApiName = apiName;
        Semaphore = new SemaphoreSlim(1, 1);
    }
}