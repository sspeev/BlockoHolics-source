using BlockoHolicsWeb.Data.Models;
using BlockoHolicsWeb.Models;
using BlockoHolicsWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static BlockoHolicsWeb.Constants.WebConstants.PortConstants;
using Timer = BlockoHolicsWeb.Services.Timer;

namespace BlockoHolicsWeb.Controllers
{
    public class HomeController(Timer timer
        , IDbService dbService) : Controller
    {
        private readonly Timer _timer = timer;
        private readonly IDbService _dbService = dbService;

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
        public async Task<IActionResult> SubmitRun(string playerName, long elapsedMs, bool isFinished)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = "Anonymous";
            }

            if (elapsedMs <= 0)
            {
                return RedirectToAction(nameof(Play));
            }

            await _dbService.WritePlayer(new Player
            {
                Name = playerName.Trim(),
                ElapsedSeconds = (int)Math.Max(1, Math.Round(elapsedMs / 1000.0)),
                IsFinished = isFinished
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
