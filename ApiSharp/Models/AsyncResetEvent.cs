namespace ApiSharp.Models;

/// <summary>
/// Async auto reset based on Stephen Toub`s implementation
/// https://devblogs.microsoft.com/pfxteam/building-async-coordination-primitives-part-2-asyncautoresetevent/
/// </summary>
/// <remarks>
/// New AsyncResetEvent
/// </remarks>
/// <param name="initialState"></param>
/// <param name="reset"></param>
public class AsyncResetEvent(bool initialState = false, bool reset = true) : IDisposable
{
    private static readonly Task<bool> _completed = Task.FromResult(true);
    private readonly ConcurrentDictionary<CancellationTokenSource, byte> _cts = new();
    private ConcurrentQueue<TaskCompletionSource<bool>> _waits = new();
    private bool _disposed;
    private bool _signaled = initialState;
    private readonly bool _reset = reset;

    /// <summary>
    /// Wait for the AsyncResetEvent to be set
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public Task<bool> WaitAsync(TimeSpan? timeout = null)
    {
        if (_signaled || _disposed)
        {
            if (_reset) _signaled = false;
            return _completed;
        }
        else
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (timeout != null)
            {
                var cancellationSource = new CancellationTokenSource(timeout.Value);
                _cts.TryAdd(cancellationSource, 0);
                var registration = cancellationSource.Token.Register(() =>
                {
                    tcs.TrySetResult(false);
                    if (_disposed) return;

                    _waits = new ConcurrentQueue<TaskCompletionSource<bool>>(_waits.Where(i => i != tcs));
                    _cts.TryRemove(cancellationSource, out _);
                }, useSynchronizationContext: false);
            }

            _waits.Enqueue(tcs);
            return tcs.Task;
        }
    }

    /// <summary>
    /// Signal a waiter
    /// </summary>
    public void Set()
    {
        if (!_reset)
        {
            // Act as ManualResetEvent. Once set keep it signaled and signal everyone who is waiting
            _signaled = true;
            while (_waits.Count > 0)
            {
                if (_waits.TryDequeue(out var toRelease) && toRelease != null)
                    toRelease.TrySetResult(true);
            }
        }
        else
        {
            // Act as AutoResetEvent. When set signal 1 waiter
            if (_waits.Count > 0)
            {
                if (_waits.TryDequeue(out var toRelease) && toRelease != null)
                    toRelease.TrySetResult(true);
            }
            else _signaled = true;
        }
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        _disposed = true;
        foreach (var cts in _cts.Keys)
        {
            cts.Dispose();
        }

#if NETSTANDARD2_1_OR_GREATER
        _cts.Clear();
        _waits.Clear();
#endif
        _waits = null;
    }
}