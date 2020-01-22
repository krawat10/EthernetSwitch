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
using Microsoft.AspNetCore.Http.Features;
using System.Net;
using EthernetSwitch.Exceptions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace EthernetSwitch.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBashCommand _bashCommand;
        private readonly ISettingsRepository _settingsRepository;

        public HomeController(ILogger<HomeController> logger, IBashCommand bashCommand,
            ISettingsRepository settingsRepository)
        {
            _logger = logger;
            _bashCommand = bashCommand;
            _settingsRepository = settingsRepository;
        }


        public IActionResult Index()
        {
            var viewModel = new IndexViewModel();

            var allVLANs = new List<string>(); // All vlan's

            var connectionLocalAddress = HttpContext.Connection.LocalIpAddress;


            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.IsEthernet())
                {
                    var isHostInterface = networkInterface
                        .GetIPProperties()
                        .UnicastAddresses
                        .Any(unicastInfo => unicastInfo.Address.Equals(connectionLocalAddress));

                    var isTagged = false; // _bashCommand.Execute($"interface {networkInterface.Name} is tagged?");
                    var appliedVLANs =
                        new List<string>(); // _bashCommand.Execute($"interface {networkInterface.Name} vlans?");
                    viewModel.Interfaces
                        .Add(new InterfaceViewModel
                        {
                            Name = networkInterface.Name,
                            Status = networkInterface.OperationalStatus,
                            IsActive = networkInterface.OperationalStatus == OperationalStatus.Up,
                            VirtualLANs = appliedVLANs, // All applied vlan's to this interface
                            AllVirtualLANs = allVLANs,
                            IsHostInterface = isHostInterface,
                            Tagged = isTagged // Check if tagged
                        });
                }
            }

            return View(viewModel);
        }

        /// <summary>
        /// Action after "Update" button click
        /// </summary>
        /// <param name="viewModel">Interface options from form.</param>
        /// <returns>Redirect to home page</returns>
        public IActionResult Edit(InterfaceViewModel viewModel)
        {
            var errors = string.Empty;
            var exitCode = 0;


            var isVlanExists = true;
            var ethInVlan = true;
            try
            {
                // var execute = _bashCommand.Execute("sudo aptitude install bridge-utils");
                // var output = _bashCommand.Execute($"brctl show br{viewModel.Name} | grep br'[0-9]' | cut -f 1");
            }
            catch (ProcessException e)
            {
                var error = e.Message;

                if (error.Contains($"br{viewModel.Name}") && error.Contains("does not exists"))
                {
                    isVlanExists = false;
                }
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

                //////////////////////////////Czy valan istnieje///////////////////////////////////////////
                 try
                 {
                var output = _bashCommand.Execute($"brctl show vlan{vlanName} | grep br'[0-9]' | cut -f 1");
                 }
                catch (ProcessException e)
                {
                    var error = e.Message;
                    if (error.Contains($"bridge vlan{vlanName} does not exist!\n"))
                    {
                    isVlanExists = false;   //true jak istnieje
                    }
                } 
                /////////////////////////////Czy interfens jest w jakimkolwiek vlanie///////////////////////
                try
                 {
                var output = _bashCommand.Execute($"brctl show | grep {viewModel.Name}");
                 }
                catch (ProcessException e)
                {
                    var error = e.ExitCode;
                    if (error == 1)
                    {
                    ethInVlan = false;   //true jak jest
                    }
                } 
                ////////////////////////////Tworzenie Vlanu nietagowanego////////////////////////////////////


                    if (isVlanExists == false & viewModel.Tagged == false )
                 	    {
                            _bashCommand.Execute($"brctl addbr vlan{vlanName}");
                 	        _bashCommand.Execute($"ip link set vlan{vlanName} up"); //stworzenie vlanu
                        } 

                ///////////////////////////Dodanie nietagowanego interfejsu do vlanu///////////////////////////
                    if (ethInVlan == true & viewModel.Tagged == false)
                    {
                        //usunięci go z vlanu do którego jest przypisany
                       var vlanID = _bashCommand.Execute($"ip link show | grep {viewModel.Name} | cut -d' ' -f9 | cut -d'n' -f2"); //pobranie numeru vlanu w którym jest interfej
                       vlanID = vlanID.Replace("\n", "");
                        _bashCommand.Execute($"ip link set vlan{vlanID} down");
                        _bashCommand.Execute($"brctl delif vlan{vlanID} {viewModel.Name}");
                    }

                    if (viewModel.Tagged==false)
                    {
                            _bashCommand.Execute($"ip link set vlan{vlanName} down");
                            _bashCommand.Execute($"brctl addif vlan{vlanName} {viewModel.Name}");
                 	        _bashCommand.Execute($"ip link set vlan{vlanName} up");
                    }
            }


            return RedirectToAction("Index");
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Settings()
        {
            var settings = _settingsRepository.GetSettings();

            var model = new SettingsViewModel
            {
                AllowRegistration = settings.AllowRegistration,
                AllowTagging = settings.AllowTagging,
                RequireConfirmation = settings.RequireConfirmation,
                NotConfirmedUsers = settings.Users
                    .Where(user => user.Role == UserRole.NotConfirmed)
                    .Select(user => user.UserName),
                AllUsers = settings.Users
                    .Where(user => user.Role != UserRole.Admin)
                    .Select(user => user.UserName)
            };

            return View("Settings", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin")]

        public IActionResult Settings(SettingsViewModel model)
        {
            var settings =_settingsRepository.GetSettings();
            
            if (ModelState.IsValid)
            {
                settings.AllowRegistration = model.AllowRegistration;
                settings.AllowTagging = model.AllowTagging;
                settings.RequireConfirmation = model.RequireConfirmation;
                
                _settingsRepository.SaveSettings(settings);

                return RedirectToAction("Settings", "Home");
            }

            return View(model);
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