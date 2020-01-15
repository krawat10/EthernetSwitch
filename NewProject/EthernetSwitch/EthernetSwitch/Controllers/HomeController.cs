using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EthernetSwitch.Models;
using EthernetSwitch.ViewModels;
using Ploeh.AutoFixture;

namespace EthernetSwitch.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFixture _fixture;

        public HomeController(ILogger<HomeController> logger, IFixture fixture)
        {
            _logger = logger;
            _fixture = fixture;
        }

        public IActionResult Index()
        {
            var interfaceViewModels = _fixture.Build<InterfaceViewModel>().With(model => model.VirtualLans, _fixture.CreateMany<VirtualLanViewModel>(3).ToList()).CreateMany(10).ToList();

            return View(_fixture.Build<IndexViewModel>()
                .With(model => model.Interfaces, interfaceViewModels).Create());
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