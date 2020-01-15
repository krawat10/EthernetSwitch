using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using EthernetSwitch.Infrastructure;
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
        private readonly IBashCommand _bashCommand;

        public HomeController(ILogger<HomeController> logger, IFixture fixture, IBashCommand bashCommand)
        {
            _logger = logger;
            _fixture = fixture;
            _bashCommand = bashCommand;
        }

        public IActionResult Index()
        {
            var interfaceViewModels = _fixture
                .Build<InterfaceViewModel>()
                .With(model => model.VirtualLans, _fixture.CreateMany<VirtualLanViewModel>(3).ToList())
                .CreateMany(10)
                .ToList();

            var viewModel = new IndexViewModel();

            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                viewModel.Interfaces
                    .Add(new InterfaceViewModel
                    {
                        Name = networkInterface.Name,
                        Status = networkInterface.OperationalStatus.ToString()
                    });
            }

            viewModel.CommadOutput = _bashCommand.Execute("ls");

            return View(viewModel);
        }

    public IActionResult Privacy()
    {
    return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
    return View(new ErrorViewModel {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
    }
}

}