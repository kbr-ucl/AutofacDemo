using System.Diagnostics;
using AutofacDemo.BusinessLogic;
using AutofacDemo.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AutofacDemo.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHelloWorld _helloWorld;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IHelloWorld helloWorld)
        {
            _logger = logger;
            _helloWorld = helloWorld;
        }

        public IActionResult Index()
        {
            ViewData["HelloWorld"] = _helloWorld.SayHello();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}