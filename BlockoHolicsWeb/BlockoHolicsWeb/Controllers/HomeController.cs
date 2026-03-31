using BlockoHolicsWeb.Models;
using BlockoHolicsWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static BlockoHolicsWeb.Constants.WebConstants.PortConstants;
using Timer = BlockoHolicsWeb.Services.Timer;

namespace BlockoHolicsWeb.Controllers
{
    public class HomeController(Timer timer) : Controller
    {
        private readonly Timer _timer = timer;

        public IActionResult Index()
        {
            _timer.Send(PortStartMessage);

            return View();
        }

        public IActionResult Privacy() => View();

        public IActionResult About() => View();

        public IActionResult Leaderboard()
        {
            var players = LeaderboardStore.GetAll();
            return View(players);
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

            var player = new Player
            {
                Name = playerName,
                ElapsedMs = elapsedMs,
                Time = timeStr
            };

            LeaderboardStore.Add(player);

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
