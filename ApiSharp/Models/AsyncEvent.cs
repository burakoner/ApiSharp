namespace ApiSharp.Models;

/// <summary>
/// Async auto reset based on Stephen Toub`s implementation
/// https://devblogs.microsoft.com/pfxteam/building-async-coordination-primitives-part-2-asyncautoresetevent/
/// </summary>
public class AsyncEvent : IDisposable
{
    private static readonly Task<bool> _completed = Task.FromResult(true);
    private readonly Queue<TaskCompletionSource<bool>> _waits = new ();
    private bool _signaled;
    private readonly bool _reset;

    public AsyncEvent(bool initialState = false, bool reset = true)
    {
        _signaled = initialState;
        _reset = reset;
    }

    public Task<bool> WaitAsync(TimeSpan? timeout = null)
    {
        lock (_waits)
        {
            if (_signaled)
            {
                if(_reset)
                    _signaled = false;
                return _completed;
            }
            else
            {
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                if(timeout != null)
                {
                    var cancellationSource = new CancellationTokenSource(timeout.Value);
                    var registration = cancellationSource.Token.Register(() =>
                    {
                        tcs.TrySetResult(false);
                    }, useSynchronizationContext: false);
                }

                _waits.Enqueue(tcs);
                return tcs.Task;
            }
        }
    }

    public void Set()
    {
        lock (_waits)
        {
            if (!_reset)
            {
                // Act as ManualResetEvent. Once set keep it signaled and signal everyone who is waiting
                _signaled = true;
                while (_waits.Count > 0)
                {
                    var toRelease = _waits.Dequeue();
                    toRelease.TrySetResult(true);
                }
            }
            else
            {
                // Act as AutoResetEvent. When set signal 1 waiter
                if (_waits.Count > 0)
                {
                    var toRelease = _waits.Dequeue();
                    toRelease.TrySetResult(true);
                }
                else if (!_signaled)
                    _signaled = true;
            }
        }
    }

    public void Dispose()
    {
        _waits.Clear();
    }
}
