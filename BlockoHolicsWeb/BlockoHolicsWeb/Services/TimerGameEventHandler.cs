using System.Diagnostics;

namespace BlockoHolicsWeb.Services;

/// <summary>
/// Handles game event recognition and stopwatch state transitions.
/// </summary>
public class TimerGameEventHandler(
    Stopwatch stopwatch,
    ILogger<Timer> logger,
    Func<TimeSpan, bool, Task> onGameStop,
    Action<TimeSpan?, DateTimeOffset?> setStoppedState)
{
    private readonly Stopwatch _stopwatch = stopwatch;
    private readonly ILogger<Timer> _logger = logger;
    private readonly Func<TimeSpan, bool, Task> _onGameStop = onGameStop;
    private readonly Action<TimeSpan?, DateTimeOffset?> _setStoppedState = setStoppedState;

    public async Task HandleEventAsync(string line)
    {
        if (IsGameStart(line))
        {
            HandleGameStart();
        }
        else if (IsGameOver(line))
        {
            await HandleGameStop(isFinished: false);
        }
        else if (IsGameWon(line))
        {
            await HandleGameStop(isFinished: true);
        }
    }

    private bool IsGameStart(string line) =>
        line.Equals("Game Started!", StringComparison.OrdinalIgnoreCase) ||
        line.Equals("Game Reset!", StringComparison.OrdinalIgnoreCase);

    private bool IsGameOver(string line) =>
        line.Equals("Game Over!", StringComparison.OrdinalIgnoreCase);

    private bool IsGameWon(string line) =>
        line.Equals("You Win!", StringComparison.OrdinalIgnoreCase);

    private void HandleGameStart()
    {
        _stopwatch.Restart();
        _setStoppedState(null, null);
        _logger.LogInformation("Game started, timer reset");
    }

    private async Task HandleGameStop(bool isFinished)
    {
        _stopwatch.Stop();
        var elapsed = _stopwatch.Elapsed;
        _setStoppedState(elapsed, DateTimeOffset.UtcNow);
        
        _logger.LogInformation("Game stopped: {Elapsed:mm\\:ss\\.ff} (Finished: {IsFinished})", 
            elapsed, isFinished);

        await _onGameStop(elapsed, isFinished);
    }
}