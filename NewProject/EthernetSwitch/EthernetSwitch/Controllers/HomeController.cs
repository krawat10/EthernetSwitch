using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using EthernetSwitch.Extensions;
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
            var viewModel = new IndexViewModel();

            var allVLANs = new List<string>(); // All vlan's

            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.IsEthernet())
                {
                    var isTagged = false;// _bashCommand.Execute($"interface {networkInterface.Name} is tagged?");
                    var appliedVLANs = new List<string>(); // _bashCommand.Execute($"interface {networkInterface.Name} vlans?");
                    
                    viewModel.Interfaces
                        .Add(new InterfaceViewModel
                        {
                            Name = networkInterface.Name,
                            Status = networkInterface.OperationalStatus,
                            IsActive = networkInterface.OperationalStatus == OperationalStatus.Up,
                            VirtualLANs = appliedVLANs, // All applied vlan's to this interface
                            AllVirtualLANs = allVLANs,  
                            Tagged = isTagged // Check if tagged
                        });
                }
            }

            // To train
            // viewModel = new IndexViewModel
            // {
            //     Interfaces = new List<InterfaceViewModel>
            //     {
            //         new InterfaceViewModel
            //         {
            //             Hidden = false,
            //             Name = "Abc",
            //             Status = OperationalStatus.Up,
            //             IsActive = true,
            //             Tagged = true,
            //             VirtualLANs = new[] {"123", "144"}, //Vland assigned to this interface
            //             AllVirtualLANs = new[] {"123", "144", "1231"} // A
            //         },
            //         new InterfaceViewModel
            //         {
            //             Hidden = false,
            //             Name = "Aerer",
            //             Status = OperationalStatus.Up,
            //             IsActive = true,
            //             Tagged = true,
            //             VirtualLANs = new[] {"123"}, //Vland assigned to this interface
            //             AllVirtualLANs = new[] {"123", "144", "1231"} // A
            //         }
            //     }
            // };

            return View(viewModel);
        }

        /// <summary>
        /// Action after "Update" button click
        /// </summary>
        /// <param name="viewModel">Interface options from form.</param>
        /// <returns>Redirect to home page</returns>
        public IActionResult Edit(InterfaceViewModel viewModel)
        {
            var model = viewModel;

            if (viewModel.IsActive) // ON-OFF checkbox
            {
                // _bashCommand.Execute($"interface up {viewModel.Name}");
            }
            else
            {
                // _bashCommand.Execute($"interface up {viewModel.Name}");
            }

            if (viewModel.Tagged) // Tag checkbox
            {
                // _bashCommand.Execute($"tag interface {viewModel.Name}");
            }
            else
            {
                // _bashCommand.Execute($"untag interface {viewModel.Name}");
            }

            foreach (var vlanName in viewModel.VirtualLANs) // All selected vlans
            {
                // 1. Check if interface exists
                // 2. Add this 
                // _bashCommand.Execute($"interface add vlan {vlanName} to {viewModel.Name}");
            }



            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}