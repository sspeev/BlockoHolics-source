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
        else if (IsGameIdle(line))
        {
            HandleGameIdle();
        }
    }

    // Timer starts only when "Game Started!" arrives (after the countdown)
    private bool IsGameStart(string line) =>
        line.Equals("Game Started!", StringComparison.OrdinalIgnoreCase);

    // Timer stops and resets for "Game Over!" (lose)
    private bool IsGameOver(string line) =>
        line.Equals("Game Over!", StringComparison.OrdinalIgnoreCase);

    // Timer stops and saves for "You Win!"
    private bool IsGameWon(string line) =>
        line.Equals("You Win!", StringComparison.OrdinalIgnoreCase);

    // Idle state: reset/ready — clear the timer, don't start it
    private bool IsGameIdle(string line) =>
        line.Equals("Game Reset!", StringComparison.OrdinalIgnoreCase) ||
        line.Equals("Game Ready!", StringComparison.OrdinalIgnoreCase);

    private void HandleGameStart()
    {
        _stopwatch.Restart();
        _setStoppedState(null, null);
        _logger.LogInformation("Game started, timer running");
    }

    private void HandleGameIdle()
    {
        _stopwatch.Reset();   // stop and zero — not just Stop()
        _setStoppedState(null, null);
        _logger.LogInformation("Game idle/reset, timer cleared");
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