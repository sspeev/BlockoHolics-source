using BlockoHolicsWeb.Contracts;
using BlockoHolicsWeb.Data.Models;
using BlockoHolicsWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Timer = BlockoHolicsWeb.Services.Timer;

namespace BlockoHolicsWeb.Controllers
{
    public class HomeController(ITimerService timer
        , IDbService dbService
        , ILogger<HomeController> _logger) : Controller
    {
        private readonly ITimerService _timer = timer;
        private readonly IDbService _dbService = dbService;
        private readonly ILogger<HomeController> _logger = _logger;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //_timer.Send(PortStartMessage);

            IList<Player> players = await _dbService.GetPlayers();
            IList<PlayerModel> playersModel = [.. players
                .Select((p, index) =>
                {
                    var elapsedMs = (long)p.ElapsedSeconds * 1000;
                    var timeSpan = TimeSpan.FromMilliseconds(elapsedMs);

                    return new PlayerModel
                    {
                        Rank = index + 1,
                        Name = p.Name,
                        ElapsedMs = elapsedMs,
                        Time = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds / 10:D2}",
                        IsFinished = p.IsFinished
                    };
                })];

            return View(playersModel);
        }

        [HttpGet]
        public IActionResult Privacy() => View();

        [HttpGet]
        public IActionResult Contacts() => View();

        [HttpGet]
        public async Task<IActionResult> Leaderboard()
        {
            IList<Player> players = await _dbService.GetPlayers();
            IList<PlayerModel> playersModel = [.. players
                .Select((p, index) =>
                {
                    var elapsedMs = (long)p.ElapsedSeconds * 1000;
                    var timeSpan = TimeSpan.FromMilliseconds(elapsedMs);

                    return new PlayerModel
                    {
                        Rank = index + 1,
                        Name = p.Name,
                        ElapsedMs = elapsedMs,
                        Time = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds / 10:D2}",
                        IsFinished = p.IsFinished
                    };
                })];

            return View(playersModel);
        }

        public IActionResult Play()
        {
            return View();
        }

        [HttpGet]
        public IActionResult StopwatchState()
        {
            var elapsed = _timer.LastStoppedElapsed ?? _timer.Elapsed;
            var latestLine = _timer.LatestLine ?? string.Empty;

            return Json(new
            {
                elapsedMs = (long)elapsed.TotalMilliseconds,
                latestLine,
                isRunning = _timer.IsRunning,
                isFinished = latestLine.Equals("You Win!", StringComparison.OrdinalIgnoreCase)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRun(SubmitRunRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var elapsedSeconds = (int)Math.Round(request.ElapsedMs / 1000.0);
            
            // Sanity checks
            if (elapsedSeconds < 1 || elapsedSeconds > 3600) // 1 sec to 1 hour max
                return BadRequest("Invalid time");

            // Check if time matches serial stopwatch (within 1 second tolerance)
            var serverTime = _timer.LastStoppedElapsed;
            if (serverTime.HasValue && 
                Math.Abs(serverTime.Value.TotalSeconds - elapsedSeconds) > 1)
            {
                _logger.LogWarning("Potential time tampering: {ClientTime}s vs {ServerTime}s", 
                    elapsedSeconds, serverTime.Value.TotalSeconds);
                return BadRequest("Time mismatch with server");
            }

            var isDuplicate = await _dbService.IsRecentRunExists((int)(elapsedSeconds), 2, request.PlayerName);
            if (isDuplicate)
                return BadRequest("Run already submitted");

            await _dbService.WritePlayer(new Player
            {
                Name = request.PlayerName.Trim(),
                ElapsedSeconds = elapsedSeconds,
                IsFinished = request.IsFinished
            });

            return RedirectToAction(nameof(Leaderboard));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
