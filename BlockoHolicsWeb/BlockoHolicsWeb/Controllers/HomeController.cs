using BlockoHolicsWeb.Models;
using Blockoholics.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BlockoHolicsWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Leaderboard()
        {
            // Temporary demo data - replace with DB or service as needed
            var players = new List<Player>
            {
                new Player { Rank = 1, Name = "CircuitMaster_X", Time = "00:45.32" },
                new Player { Rank = 2, Name = "ByteRunner", Time = "00:48.10" },
                new Player { Rank = 3, Name = "GridGlider", Time = "00:50.21" },
                new Player { Rank = 4, Name = "PixelPusher", Time = "00:55.06" },
            };

            return View(players);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
