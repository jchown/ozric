using System;
using System.Threading;
using Ozric.Engine.Live;

namespace Ozric.Engine.Utils;

/// <summary>
/// Liveness tracker for a long-lived loop. Records when it last completed an iteration
/// and the most recent exception it threw, if any. Lock-free, so a status poll on
/// another thread can read it while the loop updates it.
/// </summary>
public class Heartbeat
{
    private long _lastIterationTicks = DateTime.UtcNow.Ticks;
    private long _lastExceptionTicks;
    private volatile string? _lastExceptionMessage;

    /// <summary>Record that the loop has just completed (or begun) an iteration.</summary>
    public void Iterated() => Interlocked.Exchange(ref _lastIterationTicks, DateTime.UtcNow.Ticks);

    /// <summary>Record an exception thrown by the loop.</summary>
    public void Threw(Exception e)
    {
        _lastExceptionMessage = e.Message;
        Interlocked.Exchange(ref _lastExceptionTicks, DateTime.UtcNow.Ticks);
    }

    public DateTime LastIteration => new(Interlocked.Read(ref _lastIterationTicks), DateTimeKind.Utc);

    public TimeSpan SinceLastIteration => DateTime.UtcNow - LastIteration;

    public DateTime? LastException
    {
        get
        {
            var ticks = Interlocked.Read(ref _lastExceptionTicks);
            return ticks == 0 ? null : new DateTime(ticks, DateTimeKind.Utc);
        }
    }

    public TimeSpan? SinceLastException => LastException is { } when ? DateTime.UtcNow - when : null;

    public string? LastExceptionMessage => _lastExceptionMessage;

    /// <summary>Take a snapshot of the current liveness for display.</summary>
    public HeartbeatStatus Snapshot() => new()
    {
        sinceLastIteration = SinceLastIteration,
        sinceLastException = SinceLastException,
        lastExceptionMessage = LastExceptionMessage,
    };
}
