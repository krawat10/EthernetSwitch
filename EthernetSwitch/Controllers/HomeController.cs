using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using EthernetSwitch.Exceptions;
using EthernetSwitch.Extensions;
using EthernetSwitch.Infrastructure;
using EthernetSwitch.Models;
using EthernetSwitch.ViewModels;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EthernetSwitch.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin,User")]
    public class HomeController : Controller
    {
        private readonly IBashCommand _bashCommand;
        private readonly ILogger<HomeController> _logger;
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
            // GetRequestMessage request = new GetRequestMessage(VersionCode.V3, Messenger.NextMessageId, Messenger.NextRequestId, new OctetString("myname"), new List<variable> { new Variable(new ObjectIdentifier("1.3.6.1.2.1.1.1.0")) }, priv, Messenger.MaxMessageSize, report);
            // ISnmpMessage reply = request.GetResponse(60000, new IPEndPoint(IPAddress.Parse("192.168.1.2"), 161));
            // if (reply.Pdu().ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
            // {
            //     throw ErrorException.Create(
            //         "error in response",
            //         IPAddress.Parse("192.168.1.2"),
            //         reply);
            // }

            var settings = _settingsRepository.GetSettings();
            var allowTagging = settings.AllowTagging;

            var viewModel = new IndexViewModel();

            var allVLANs = new List<string>(); // All vlan's

            var connectionLocalAddress = HttpContext.Connection.LocalIpAddress;

            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                if (networkInterface.IsEthernet())
                {
                    var output =
                        _bashCommand.Execute(
                            $"ip link show | grep {networkInterface.Name}| grep vlan | cut -d' ' -f9 | cut -d'n' -f2");


                    var appliedVLANs = output
                        .Replace("\t", string.Empty)
                        .Replace(networkInterface.Name, string.Empty)
                        .Split('\n')
                        .Select(vlan => vlan.Trim('.'))
                        .Where(vlan => !string.IsNullOrWhiteSpace(vlan))
                        .Where(vlan => !vlan.ToLower().Equals("down"))
                        .ToList();

                    var isHostInterface = networkInterface
                        .GetIPProperties()
                        .UnicastAddresses
                        .Any(unicastInfo => unicastInfo.Address.Equals(connectionLocalAddress));

                    var tagged = true;
                    try
                    {
                        _bashCommand.Execute($"ip link show | grep @{networkInterface.Name}");
                    }
                    catch (ProcessException e)
                    {
                        var error = e.ExitCode;
                        if (error == 1) tagged = false;
                    }


                    //Checks if interface is in isolated mode
                    var isIsolated = true;

                    try
                    {
                        _bashCommand.Execute($"ebtables -L | grep DROP | cut -d' ' -f2 | grep {networkInterface.Name}");
                    }
                    catch (ProcessException e)
                    {
                        var error = e.ExitCode;
                        if (error == 1) isIsolated = false;
                    }

                    // Checks if interface is in promiscuous mode
                    var isPromiscuous = true;

                    try
                    {
                        _bashCommand.Execute(
                            $"ebtables -L | grep ACCEPT | cut -d' ' -f4 | grep {networkInterface.Name}");
                    }
                    catch (ProcessException e)
                    {
                        var error = e.ExitCode;
                        if (error == 1) isPromiscuous = false;
                    }

                    var type = InterfaceType.Off;
                    if (isIsolated) type = InterfaceType.Isolated;

                    if (isPromiscuous) type = InterfaceType.Promiscuous;

                    viewModel.Interfaces
                        .Add(new InterfaceViewModel
                        {
                            Name = networkInterface.Name,
                            Status = networkInterface.OperationalStatus,
                            IsActive = networkInterface.OperationalStatus == OperationalStatus.Up,
                            VirtualLANs = appliedVLANs, // All applied VLANs to this interface
                            AllVirtualLANs = allVLANs,
                            IsHostInterface = isHostInterface,
                            Tagged = tagged,
                            AllowTagging = allowTagging,
                            Type = type
                        });
                }

            return View(viewModel);
        }

        /// <summary>
        ///     Action after "Update" button click
        /// </summary>
        /// <param name="viewModel">Interface options from form.</param>
        /// <returns>Redirect to home page</returns>
        public IActionResult Edit(InterfaceViewModel viewModel)
        {
            if (viewModel.IsActive)
                _bashCommand.Execute($"ip link set {viewModel.Name} up");
            else
                _bashCommand.Execute($"ip link set {viewModel.Name} down");


            // Gets interface config
            var output2 =
                _bashCommand.Execute(
                    $"ip link show | grep {viewModel.Name}| grep vlan | cut -d' ' -f9 | cut -d'n' -f2");


            var VLANsToRemove = output2
                .Replace("\t", string.Empty)
                .Replace(viewModel.Name, string.Empty)
                .Split('\n')
                .Select(vlan => vlan.Trim('.'))
                .Where(vlan => !string.IsNullOrWhiteSpace(vlan))
                .ToList();


            foreach (var vlanName in VLANsToRemove) // All selected vlans
            {
                // Tagged interfaces
                var ifToRemIsTaged = true;
                try
                {
                    var output = _bashCommand.Execute($"ip link show {viewModel.Name}.{vlanName}");
                }
                catch (ProcessException e)
                {
                    var error = e.Message;
                    if (error.Contains("does not exist.\n")) ifToRemIsTaged = false;
                }

                if (ifToRemIsTaged)
                {
                    _bashCommand.Execute($"ip link set {viewModel.Name}.{vlanName} down"); // Off interface
                    _bashCommand.Execute($"ip link delete {viewModel.Name}.{vlanName}");
                }
                else //Non-tagged
                {
                    _bashCommand.Execute($"ip link set vlan{vlanName} down");
                    _bashCommand.Execute($"brctl delif vlan{vlanName} {viewModel.Name}");
                    _bashCommand.Execute($"ip link set vlan{vlanName} up");
                }

                // Clears empty bridged
                var output3 = _bashCommand.Execute($"brctl show vlan{vlanName} | grep vlan{vlanName} | cut -f6");

                if (output3 == "\n")
                {
                    _bashCommand.Execute($"ip link set vlan{vlanName} down");
                    _bashCommand.Execute($"ip link delete vlan{vlanName}");
                }
            }


            foreach (var vlanName in viewModel.VirtualLANs) // All selected VLANs
            {
                // Checks if VLAN exists
                var vlanExists = true;
                try
                {
                    _bashCommand.Execute($"brctl show vlan{vlanName}");
                }
                catch (ProcessException e)
                {
                    var error = e.Message;
                    if (error.Contains($"bridge vlan{vlanName} does not exist!\n")) vlanExists = false; //true if exists
                }

                // Checks if interface is in any VLAN
                var interfaceHasVLAN = true;
                try
                {
                    var output = _bashCommand.Execute($"brctl show | grep {viewModel.Name}");
                }
                catch (ProcessException e)
                {
                    var error = e.ExitCode;
                    if (error == 1) interfaceHasVLAN = false;
                }

                // Creates VLAN
                if (!vlanExists)
                {
                    _bashCommand.Execute($"brctl addbr vlan{vlanName}");
                    _bashCommand.Execute($"ip link set vlan{vlanName} up"); //Create VLAN
                }

                // Adds non-tagged interface to VLAN
                if (interfaceHasVLAN & (viewModel.Tagged == false))
                {
                    // Removes from VLAN which is assigned to
                    var vlanID =
                        _bashCommand.Execute(
                            $"ip link show | grep [[:space:]]{viewModel.Name}: | cut -d' ' -f9 | cut -d'n' -f2"); // Gets VLAN numer
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

                //Creates tagged interface
                if (viewModel.Tagged)
                {
                    _bashCommand.Execute($"ip link set vlan{vlanName} down");
                    _bashCommand.Execute(
                        $"ip link add link {viewModel.Name} name {viewModel.Name}.{vlanName} type vlan id {vlanName}");
                    _bashCommand.Execute($"ip link set vlan{vlanName} up");
                }

                //Adds tagged interface to VLAN
                if (viewModel.Tagged)
                {
                    _bashCommand.Execute($"ip link set {viewModel.Name} up");

                    _bashCommand.Execute($"ip link set vlan{vlanName} down");
                    _bashCommand.Execute($"brctl addif vlan{vlanName} {viewModel.Name}.{vlanName}");
                    _bashCommand.Execute($"ip link set {viewModel.Name}.{vlanName} up");
                    _bashCommand.Execute($"ip link set vlan{vlanName} up");
                }
            }

            //Private VLAN
            switch (viewModel.Type)
            {
                case InterfaceType.Off:

                    foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                        if (networkInterface.IsEthernet())
                        {
                            //Clear interface drop rules
                            try
                            {
                                var output = _bashCommand.Execute(
                                    $"ebtables -D FORWARD -i {viewModel.Name} -o {networkInterface.Name} -j DROP");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }

                            //Clear interface acceptation rules
                            try
                            {
                                var output = _bashCommand.Execute(
                                    $"ebtables -D FORWARD -i {networkInterface.Name} -o {viewModel.Name} -j ACCEPT");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }
                        }

                    break;

                case InterfaceType.Isolated:

                    foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                        if (networkInterface.IsEthernet())
                        {
                            //Clear interface drop rules
                            try
                            {
                                var output = _bashCommand.Execute(
                                    $"ebtables -D FORWARD -i {viewModel.Name} -o {networkInterface.Name} -j DROP");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }

                            //Clear interface acceptation rules
                            try
                            {
                                var output = _bashCommand.Execute(
                                    $"ebtables -D FORWARD -i {networkInterface.Name}  -o  {viewModel.Name} -j ACCEPT");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }

                            // Block access
                            if (viewModel.Name != networkInterface.Name)
                                try
                                {
                                    var output = _bashCommand.Execute(
                                        $"ebtables -A FORWARD -i {viewModel.Name} -o {networkInterface.Name} -j DROP");
                                }
                                catch (ProcessException e)
                                {
                                    var error = e.ExitCode;
                                }
                        }


                    break;
                case InterfaceType.Promiscuous:
                    foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                        if (networkInterface.IsEthernet())
                        {
                            // Clears interface drop rules
                            try
                            {
                                var output = _bashCommand.Execute(
                                    $"ebtables -D FORWARD -i {viewModel.Name} -o {networkInterface.Name} -j DROP");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }

                            //Clear interface acceptation rules
                            try
                            {
                                var output = _bashCommand.Execute(
                                    $"ebtables -D FORWARD -i {networkInterface.Name} -o {viewModel.Name} -j ACCEPT");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }

                            // Grants access
                            if (viewModel.Name != networkInterface.Name)
                                try
                                {
                                    _bashCommand.Execute(
                                        $"ebtables -I FORWARD -i {networkInterface.Name} -o {viewModel.Name} -j ACCEPT");
                                }
                                catch (ProcessException e)
                                {
                                    var error = e.ExitCode;
                                }
                        }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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