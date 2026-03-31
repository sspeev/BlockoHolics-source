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

        public IActionResult Privacy() => View();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitRun(string playerName, long elapsedMs)
        {
            if (string.IsNullOrWhiteSpace(playerName)) playerName = "Anonymous";

            var timeSpan = System.TimeSpan.FromMilliseconds(elapsedMs);
            string timeStr = string.Format("{0:D2}:{1:D2}.{2:D2}",
                timeSpan.Minutes,
                timeSpan.Seconds,
                timeSpan.Milliseconds / 10);

            var player = new PlayerModel
            {
                Name = playerName,
                ElapsedMs = elapsedMs,
                Time = timeStr
            };


            return RedirectToAction("Leaderboard");
        }

        public IActionResult Play()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
