using System.Diagnostics;
using System.IO.Ports;
using BlockoHolicsWeb.Contracts;
using BlockoHolicsWeb.Data.Models;

namespace BlockoHolicsWeb.Services;

/// <summary>
/// Background service that monitors Arduino serial port for game events.
/// Tracks elapsed time and persists results to database.
/// </summary>
public class Timer : BackgroundService, ITimerService
{
    private readonly SerialPort _serialPort;
    private readonly ILogger<Timer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Lock _sync = new();
    private readonly Stopwatch _stopwatch = new();
    private readonly TimerGameEventHandler _eventHandler;

    private string? _latestLine;
    private TimeSpan? _lastStoppedElapsed;
    private DateTimeOffset? _lastStoppedAtUtc;

    public Timer(SerialPort serialPort, ILogger<Timer> logger, IServiceScopeFactory scopeFactory)
    {
        _serialPort = serialPort;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _eventHandler = new TimerGameEventHandler(
            _stopwatch,
            _logger,
            SaveToDatabase,
            SetStoppedState);
    }

    // ============ ITimerService Implementation ============

    public string? LatestLine => GetThreadSafe(() => _latestLine);
    public TimeSpan Elapsed => _stopwatch.Elapsed;
    public bool IsRunning => _stopwatch.IsRunning;
    public TimeSpan? LastStoppedElapsed => GetThreadSafe(() => _lastStoppedElapsed);
    public DateTimeOffset? LastStoppedAtUtc => GetThreadSafe(() => _lastStoppedAtUtc);

    public void Send(string command)
    {
        try
        {
            if (!_serialPort.IsOpen)
                _serialPort.Open();

            _serialPort.WriteLine(command);
            _logger.LogDebug("Serial sent: {Command}", command);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Failed to send command on {Port}", _serialPort.PortName);
        }
    }

    // ============ Background Service Loop ============

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timer service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!TryEnsurePortOpen())
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            try
            {
                var line = _serialPort.ReadLine()?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(line))
                    continue;

                SetThreadSafe(() => _latestLine = line);
                await _eventHandler.HandleEventAsync(line);
            }
            catch (TimeoutException)
            {
                // Expected when no data available
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or InvalidOperationException)
            {
                _logger.LogError(ex, "Serial port error on {Port}. Retrying...", _serialPort.PortName);
                ClosePort();
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected serial read error");
                await Task.Delay(500, stoppingToken);
            }
        }
    }

    // ============ Private Helpers ============

    private void SetStoppedState(TimeSpan? elapsed, DateTimeOffset? stoppedAt)
    {
        SetThreadSafe(() =>
        {
            _lastStoppedElapsed = elapsed;
            _lastStoppedAtUtc = stoppedAt;
        });
    }

    private async Task SaveToDatabase(TimeSpan elapsed, bool isFinished)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbService = scope.ServiceProvider.GetRequiredService<IDbService>();

            await dbService.WritePlayer(new Player
            {
                ElapsedSeconds = (int)elapsed.TotalSeconds,
                IsFinished = isFinished
            });

            _logger.LogInformation("Run saved to database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save run to database");
        }
    }

    private T? GetThreadSafe<T>(Func<T?> getter)
    {
        lock (_sync)
            return getter();
    }

    private void SetThreadSafe(Action setter)
    {
        lock (_sync)
            setter();
    }

    private bool TryEnsurePortOpen()
    {
        try
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
                _logger.LogInformation("Opened serial port: {Port}", _serialPort.PortName);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Serial port {Port} unavailable", _serialPort.PortName);
            return false;
        }
    }

    private void ClosePort()
    {
        try
        {
            if (_serialPort.IsOpen)
                _serialPort.Close();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error closing port");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        ClosePort();
        await base.StopAsync(cancellationToken);
    }
}
