namespace BlockoHolicsWeb.Contracts;

public interface ITimerGameEventHandlerService
{
    string? LatestLine { get; }
    TimeSpan Elapsed { get; }
    bool IsRunning { get; }
    TimeSpan? LastStoppedElapsed { get; }
    DateTimeOffset? LastStoppedAtUtc { get; }
    void Send(string command);
}