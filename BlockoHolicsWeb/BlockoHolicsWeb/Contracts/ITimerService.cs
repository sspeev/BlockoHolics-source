namespace BlockoHolicsWeb.Contracts;

/// <summary>
/// Monitors Arduino serial port for game events and manages stopwatch.
/// </summary>
public interface ITimerService
{
    /// <summary>Gets the latest serial message received.</summary>
    string? LatestLine { get; }

    /// <summary>Gets current elapsed time (whether running or stopped).</summary>
    TimeSpan Elapsed { get; }

    /// <summary>Gets whether the timer is currently running.</summary>
    bool IsRunning { get; }

    /// <summary>Gets the elapsed time when timer last stopped.</summary>
    TimeSpan? LastStoppedElapsed { get; }

    /// <summary>Gets the UTC timestamp when timer last stopped.</summary>
    DateTimeOffset? LastStoppedAtUtc { get; }

    /// <summary>Sends a command to the Arduino via serial port.</summary>
    void Send(string command);
}