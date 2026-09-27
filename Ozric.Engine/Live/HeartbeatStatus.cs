using System;

namespace Ozric.Engine.Live;

/// <summary>
/// A point-in-time snapshot of a long-lived loop's liveness, suitable for display.
/// </summary>
public class HeartbeatStatus
{
    /// <summary>How long since the loop last completed an iteration.</summary>
    public TimeSpan sinceLastIteration { get; init; }

    /// <summary>How long since the loop last threw an exception, or null if it never has.</summary>
    public TimeSpan? sinceLastException { get; init; }

    /// <summary>The message of the most recent exception, or null if none.</summary>
    public string? lastExceptionMessage { get; init; }
}
