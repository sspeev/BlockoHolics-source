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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
