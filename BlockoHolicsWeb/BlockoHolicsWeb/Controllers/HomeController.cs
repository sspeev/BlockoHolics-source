using BlockoHolicsWeb.Contracts;
using BlockoHolicsWeb.Data.Models;
using BlockoHolicsWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BlockoHolicsWeb.Controllers
{
    public class HomeController(ITimerService timer
        , IDbService dbService
        , ILogger<HomeController> logger) : Controller
    {
        private readonly ITimerService _timer = timer;
        private readonly IDbService _dbService = dbService;
        private readonly ILogger<HomeController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IList<Player> players = await _dbService.GetPlayers();
            IList<PlayerModel> playersModel = [.. players
                .Select((p, index) =>
                {
                    var elapsedMs = p.ElapsedSeconds * 1000d;

                    return new PlayerModel
                    {
                        Rank = index + 1,
                        Name = p.Name,
                        ElapsedMs = elapsedMs,
                        Time = $"{p.ElapsedSeconds:F3}s", // seconds + milliseconds
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
                    var elapsedMs = p.ElapsedSeconds * 1000d;

                    return new PlayerModel
                    {
                        Rank = index + 1,
                        Name = p.Name,
                        ElapsedMs = elapsedMs,
                        Time = $"{p.ElapsedSeconds:F3}s", // seconds + milliseconds
                        IsFinished = p.IsFinished
                    };
                })];

            return View(playersModel);
        }

        [HttpGet]
        public IActionResult Play() => View();

        [HttpGet]
        public IActionResult StopwatchState()
        {
            var latestLine = _timer.LatestLine ?? string.Empty;
            var isRunning  = _timer.IsRunning;

            // When running: return the live ticking elapsed time.
            // When stopped: return the frozen value captured at stop time.
            var elapsed = isRunning
                ? _timer.Elapsed
                : (_timer.LastStoppedElapsed ?? TimeSpan.Zero);

            return Json(new
            {
                elapsedMs  = (long)elapsed.TotalMilliseconds,
                latestLine,
                isRunning,
                isFinished = latestLine.Equals("You Win!", StringComparison.OrdinalIgnoreCase)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRun(SubmitRunRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var elapsedSeconds = request.ElapsedMs / 1000.0;

            if (elapsedSeconds < 1 || elapsedSeconds > 3600)
                return BadRequest("Invalid time");

            var serverTime = _timer.LastStoppedElapsed;
            if (serverTime.HasValue &&
                Math.Abs(serverTime.Value.TotalSeconds - elapsedSeconds) > 1)
            {
                _logger.LogWarning("Potential time tampering: {ClientTime}s vs {ServerTime}s",
                    elapsedSeconds, serverTime.Value.TotalSeconds);
                return BadRequest("Time mismatch with server");
            }

            var isDuplicate = await _dbService.IsRecentRunExists(elapsedSeconds, 2, request.PlayerName);
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
