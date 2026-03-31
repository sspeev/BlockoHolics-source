using System.Diagnostics;
using System.IO.Ports;
using Microsoft.Extensions.DependencyInjection;
using Player = BlockoHolicsWeb.Data.Models.Player;

namespace BlockoHolicsWeb.Services;

public class Timer(
      SerialPort serialPort
    , ILogger<Timer> logger
    , IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly SerialPort _serialPort = serialPort;
    private readonly ILogger<Timer> _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly Lock _sync = new();
    private readonly Stopwatch _stopwatch = new();

    private string? _latestLine;
    private TimeSpan? _lastStoppedElapsed;
    private DateTimeOffset? _lastStoppedAtUtc;

    public string? LatestLine
    {
        get
        {
            lock (_sync)
            {
                return _latestLine;
            }
        }
    }

    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public bool IsRunning => _stopwatch.IsRunning;

    public TimeSpan? LastStoppedElapsed
    {
        get
        {
            lock (_sync)
            {
                return _lastStoppedElapsed;
            }
        }
    }

    public DateTimeOffset? LastStoppedAtUtc
    {
        get
        {
            lock (_sync)
            {
                return _lastStoppedAtUtc;
            }
        }
    }

    public void Send(string command)
    {
        try
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }

            _serialPort.WriteLine(command);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Unable to send serial command. Port {PortName} is unavailable.", _serialPort.PortName);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!TryEnsurePortOpen())
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            try
            {
                var line = _serialPort.ReadLine().Trim();

                lock (_sync)
                {
                    _latestLine = line;
                }

                if (line.Equals("Game Started!", StringComparison.OrdinalIgnoreCase)
                    || line.Equals("Game Reset!", StringComparison.OrdinalIgnoreCase))
                {
                    _stopwatch.Restart();

                    lock (_sync)
                    {
                        _lastStoppedElapsed = null;
                        _lastStoppedAtUtc = null;
                    }

                    _logger.LogInformation("Stopwatch started.");
                }

                if (line.Equals("Game Over!", StringComparison.OrdinalIgnoreCase))
                {
                    await StopAndSave(false);
                }
                if (line.Equals("You Win!", StringComparison.OrdinalIgnoreCase))
                {
                    await StopAndSave(true);
                }
                _logger.LogInformation("Serial: {Line}", line);
            }
            catch (TimeoutException)
            {
                // expected when no data is available yet
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or InvalidOperationException)
            {
                _logger.LogError(ex, "Serial port failure on {PortName}. Will retry.", _serialPort.PortName);

                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }

                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Serial read failed.");
                await Task.Delay(500, stoppingToken);
            }
        }
    }

    private async Task StopAndSave(bool isFinished)
    {
        _stopwatch.Stop();

        lock (_sync)
        {
            _lastStoppedElapsed = _stopwatch.Elapsed;
            _lastStoppedAtUtc = DateTimeOffset.UtcNow;
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbService = scope.ServiceProvider.GetRequiredService<IDbService>();

        await dbService.WritePlayer(new Player
        {
            ElapsedSeconds = (int)_stopwatch.Elapsed.TotalSeconds,
            IsFinished = isFinished
        });

        _logger.LogInformation("Stopwatch stopped at {Elapsed} ({StoppedAtUtc:u}).", _stopwatch.Elapsed, _lastStoppedAtUtc);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        if (_serialPort.IsOpen)
        {
            _serialPort.Close();
        }

        return base.StopAsync(cancellationToken);
    }

    private bool TryEnsurePortOpen()
    {
        try
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
                _logger.LogInformation("Opened serial port {PortName}.", _serialPort.PortName);
            }

            return true;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Serial port {PortName} not available.", _serialPort.PortName);
            return false;
        }
    }
}
