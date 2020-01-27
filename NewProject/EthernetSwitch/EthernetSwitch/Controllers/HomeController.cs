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
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin,User")]
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
            var settings = _settingsRepository.GetSettings();
            var allowTagging = settings.AllowTagging;

            var viewModel = new IndexViewModel();

            var allVLANs = new List<string>(); // All vlan's

            var connectionLocalAddress = HttpContext.Connection.LocalIpAddress;

            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.IsEthernet())
                {
                     
                    var output =
                        _bashCommand.Execute(
                            $"ip link show | grep {networkInterface.Name}| grep vlan | cut -d' ' -f9 | cut -d'n' -f2");


                    var appliedVLANs = output
                        .Replace("\t", String.Empty)
                        .Replace(networkInterface.Name, String.Empty)
                        .Split('\n')
                        .Select(vlan => vlan.Trim('.'))
                        .Where(vlan => !string.IsNullOrWhiteSpace(vlan))
                        .Where(vlan => !vlan.ToLower().Equals("down"))
                        .ToList();

                    var isHostInterface = networkInterface
                        .GetIPProperties()
                        .UnicastAddresses
                        .Any(unicastInfo => unicastInfo.Address.Equals(connectionLocalAddress));

                    var isTagged = false; // _bashCommand.Execute($"interface {networkInterface.Name} is tagged?");
                   
                    var tagged = true;
                    try
                    {
                        var findtag = _bashCommand.Execute($"ip link show | grep @{networkInterface.Name}");
                    }
                    catch (ProcessException e)
                    {
                        var error = e.ExitCode;
                        if (error == 1)
                        {
                            tagged = false;
                        }
                    }
                    ///////////////////////////////////////czy interfejs jest w isolated mode/////////////////////////////
                    

                    var isolated = true;

                    try
                    {
                        var findtag = _bashCommand.Execute($"ebtables -L | grep DROP | cut -d' ' -f2 | grep {networkInterface.Name}");
                    }
                    catch (ProcessException e)
                    {
                        var error = e.ExitCode;
                        if (error == 1)
                        {
                            isolated = false;
                        }
                    }

                    ///////////////////////////////////////czy interfejs jest w Promiscuous mode/////////////////////////////
                       var promiscuous = true;

                    try
                    {
                        var findtag = _bashCommand.Execute($"ebtables -L | grep ACCEPT | cut -d' ' -f4 | grep {networkInterface.Name}");
                    }
                    catch (ProcessException e)
                    {
                        var error = e.ExitCode;
                        if (error == 1)
                        {
                            promiscuous = false;
                        }
                    }
                    var type = InterfaceType.Off;
                    if (isolated == true)
                    {
                        type = InterfaceType.Isolated;
                    }
                     if (promiscuous == true)
                    {
                        type = InterfaceType.Promiscuous;
                    }

                    viewModel.Interfaces
                        .Add(new InterfaceViewModel
                        {
                            Name = networkInterface.Name,
                            Status = networkInterface.OperationalStatus,
                            IsActive = networkInterface.OperationalStatus == OperationalStatus.Up,
                            VirtualLANs = appliedVLANs, // All applied vlan's to this interface
                            AllVirtualLANs = allVLANs,
                            IsHostInterface = isHostInterface,
                            Tagged = tagged, //isTagged, // Check if tagged
                            AllowTagging = allowTagging,
                            Type=type
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


              if (viewModel.IsActive == true)
                    {
                        _bashCommand.Execute($"ip link set {viewModel.Name} up");
                    } else
                    {
                        _bashCommand.Execute($"ip link set {viewModel.Name} down");
                    }
            
            if (viewModel.Tagged) // Tag checkbox
            {
                // _bashCommand.Execute($"tag interface {viewModel.Name}");
            }
            else
            {
                // _bashCommand.Execute($"untag interface {viewModel.Name}");
            }
            
            ///////////////////////////////////////oranie konfiguracji interfejsu/////////////////////////////////
           
            
             var output2 =
                        _bashCommand.Execute(
                            $"ip link show | grep {viewModel.Name}| grep vlan | cut -d' ' -f9 | cut -d'n' -f2");


                    var VLANsToRemove = output2
                        .Replace("\t", String.Empty)
                        .Replace(viewModel.Name, String.Empty)
                        .Split('\n')
                        .Select(vlan => vlan.Trim('.'))
                        .Where(vlan => !string.IsNullOrWhiteSpace(vlan))
                        .ToList();



              foreach (var vlanName in VLANsToRemove) // All selected vlans
            {
                //////////////////////////////////////////////tagowane////////////////////////////////////////////////
                var ifToRemIsTaged = true;
                try
                {
                    var output = _bashCommand.Execute($"ip link show {viewModel.Name}.{vlanName}");
                }
                catch (ProcessException e)
                {
                    var error = e.Message;
                    if (error.Contains($"does not exist.\n"))
                    {
                        ifToRemIsTaged = false;
                    } 
                }

                if(ifToRemIsTaged == true)
                {
                    _bashCommand.Execute($"ip link set {viewModel.Name}.{vlanName} down");  //usuwanie intrfejsów
                    _bashCommand.Execute($"ip link delete {viewModel.Name}.{vlanName}");
                } else
                /////////////////////////////////////////////nietagowane///////////////////////////////////////////////
                {
                    _bashCommand.Execute($"ip link set vlan{vlanName} down");
                    _bashCommand.Execute($"brctl delif vlan{vlanName} {viewModel.Name}");
                    _bashCommand.Execute($"ip link set vlan{vlanName} up");
                }

                ////////////////////////////////////////////usuwanie pustych br////////////////////////////////////////
                try
                {
                    var output = _bashCommand.Execute($"brctl show vlan{vlanName} | grep eth");
                }
                catch (ProcessException e)
                {
                    var error = e.ExitCode;
                    if (error == 1)
                    {
                        _bashCommand.Execute($"ip link set vlan{vlanName} down");
                        _bashCommand.Execute($"ip link delete vlan{vlanName}");
                    }
                }
            }
            
            
                    
            foreach (var vlanName in viewModel.VirtualLANs) // All selected vlans
            {

                // 1. Check if interface exists
                // 2. Add this 
                // _bashCommand.Execute($"interface add vlan {vlanName} to {viewModel.Name}");
              
                //////////////////////////////Czy valan istnieje///////////////////////////////////////////OK
                var vlanExists = true;
                try
                {
                    var output = _bashCommand.Execute($"brctl show vlan{vlanName}");
                }
                catch (ProcessException e)
                {
                    var error = e.Message;
                    if (error.Contains($"bridge vlan{vlanName} does not exist!\n"))
                    {
                        vlanExists = false; //true jak istnieje
                    }
                }

                /////////////////////////////Czy interfens jest w jakimkolwiek vlanie///////////////////////OK
                var intervaceHasVlan = true;
                try
                {
                    var output = _bashCommand.Execute($"brctl show | grep {viewModel.Name}");
                }
                catch (ProcessException e)
                {
                    var error = e.ExitCode;
                    if (error == 1)
                    {
                        intervaceHasVlan = false; //true jak jest
                    }
                }

                ////////////////////////////Tworzenie Vlanu////////////////////////////////////
                if (!vlanExists)
                {
                    _bashCommand.Execute($"brctl addbr vlan{vlanName}");
                    _bashCommand.Execute($"ip link set vlan{vlanName} up"); //stworzenie vlanu
                }

                ///////////////////////////Dodanie nietagowanego interfejsu do vlanu///////////////////////////
                if (intervaceHasVlan & viewModel.Tagged == false)
                {
                //usunięci go z vlanu do którego jest przypisany
                    var vlanID =
                        _bashCommand.Execute(
                            $"ip link show | grep [[:space:]]{viewModel.Name}: | cut -d' ' -f9 | cut -d'n' -f2"); //pobranie numeru vlanu w którym jest interfej
                    vlanID = vlanID.Replace("\n", "");
                    _bashCommand.Execute($"ip link set vlan{vlanID} down");
                    _bashCommand.Execute($"brctl delif vlan{vlanID} {viewModel.Name}");
                }

                if (!viewModel.Tagged)
                {
                    _bashCommand.Execute($"ip link set vlan{vlanName} down");
                    _bashCommand.Execute($"brctl addif vlan{vlanName} {viewModel.Name}");
                    _bashCommand.Execute($"ip link set vlan{vlanName} up");
                }
                ///////////////////////////Tworzenie tagowanego interfejsu///////////////////////////
                 if (viewModel.Tagged)
                {
                    _bashCommand.Execute($"ip link set vlan{vlanName} down");
                    _bashCommand.Execute($"ip link add link {viewModel.Name} name {viewModel.Name}.{vlanName} type vlan id {vlanName}");
                    _bashCommand.Execute($"ip link set vlan{vlanName} up");
                }
                ///////////////////////////Dodanie tahowanego interfejsu do vlanu///////////////////////////
                if (viewModel.Tagged)
                {
                    _bashCommand.Execute($"ip link set {viewModel.Name} up");

                    _bashCommand.Execute($"ip link set vlan{vlanName} down");
                    _bashCommand.Execute($"brctl addif vlan{vlanName} {viewModel.Name}.{vlanName}");
                    _bashCommand.Execute($"ip link set {viewModel.Name}.{vlanName} up");
                    _bashCommand.Execute($"ip link set vlan{vlanName} up");
                }
            }

            ///////////////////////////////Private vlan/////////////////////////////////////////////////////
            switch (viewModel.Type)
            {
                case InterfaceType.Off:

                    foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (networkInterface.IsEthernet())
                        {
                            //////////////////////////////////////////////czyszczenie regół drop dot interfejsu/////////////////////////////////////////////
                            try
                            {
                                var output = _bashCommand.Execute($"ebtables -D FORWARD -i {viewModel.Name} -o {networkInterface.Name} -j DROP");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }   
                                
                            //////////////////////////////////////////////czyszczenie regół accept dot interfejsu/////////////////////////////////////////////
                                        try
                            {
                                var output = _bashCommand.Execute($"ebtables -D FORWARD -i {networkInterface.Name} -o {viewModel.Name} -j ACCEPT");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }   
                                
                        }
                    } 
                    break;

                case InterfaceType.Isolated:

                    foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (networkInterface.IsEthernet())
                        {
                            //////////////////////////////////////////////czyszczenie regół drop dot interfejsu/////////////////////////////////////////////
                            try
                            {
                                var output = _bashCommand.Execute($"ebtables -D FORWARD -i {viewModel.Name} -o {networkInterface.Name} -j DROP");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }   
                               
                            //////////////////////////////////////////////czyszczenie regół accept dot interfejsu/////////////////////////////////////////////
                                        try
                            {
                                var output = _bashCommand.Execute($"ebtables -D FORWARD -i {networkInterface.Name}  -o  {viewModel.Name} -j ACCEPT");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }   
                            //////////////////////////////////////////////blokowanie dostępu/////////////////////////////////////////////
                            if (viewModel.Name != networkInterface.Name)
                            {
                                try
                                {
                                    var output = _bashCommand.Execute($"ebtables -A FORWARD -i {viewModel.Name} -o {networkInterface.Name} -j DROP");
                                }
                                catch (ProcessException e)
                                {
                                    var error = e.ExitCode;
                                }   
                            }
                        }
                    } 


                    break;
                case InterfaceType.Promiscuous:
                                       foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (networkInterface.IsEthernet())
                        {
                            //////////////////////////////////////////////czyszczenie regół drop dot interfejsu/////////////////////////////////////////////
                            try
                            {
                                var output = _bashCommand.Execute($"ebtables -D FORWARD -i {viewModel.Name} -o {networkInterface.Name} -j DROP");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }   
                               
                            //////////////////////////////////////////////czyszczenie regół accept dot interfejsu/////////////////////////////////////////////
                                        try
                            {
                                var output = _bashCommand.Execute($"ebtables -D FORWARD -i {networkInterface.Name} -o {viewModel.Name} -j ACCEPT");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }   
                            //////////////////////////////////////////////udzielanie dostepu/////////////////////////////////////////////
                            if (viewModel.Name != networkInterface.Name)
                            {
                                try
                                {
                                    var output = _bashCommand.Execute($"ebtables -I FORWARD -i {networkInterface.Name} -o {viewModel.Name} -j ACCEPT");
                                }
                                catch (ProcessException e)
                                {
                                    var error = e.ExitCode;
                                }   
                            }
                        }
                    } 

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////
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
            var settings = _settingsRepository.GetSettings();

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