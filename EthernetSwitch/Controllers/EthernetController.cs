using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using EthernetSwitch.Infrastructure.Ethernet;
using EthernetSwitch.Infrastructure.Extensions;
using EthernetSwitch.Infrastructure.Settings;
using EthernetSwitch.Models;
using EthernetSwitch.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceStack;

namespace EthernetSwitch.Controllers {

    [Authorize (AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin,User")]
    public class EthernetController : Controller {
        private readonly EthernetServices _ethernetServices;
        private readonly LLDPServices _lldpSercives;
        private readonly ILogger<EthernetController> _logger;
        private readonly ISettingsRepository _settingsRepository;

        public EthernetController(ILogger<EthernetController> logger, EthernetServices ethernetServices, LLDPServices lldpSercives,
            ISettingsRepository settingsRepository) {
            _logger = logger;
            _ethernetServices = ethernetServices;
            _lldpSercives = lldpSercives;
            _settingsRepository = settingsRepository;
        }

        public async Task<IActionResult> Index (IndexViewModel viewModel) {
            var settings = await _settingsRepository.GetSettings ();
            var neighbours = new List<EthernetNeighbor>();
            viewModel ??= new IndexViewModel();

            try
            {
                _lldpSercives.ActivateLLDPAgent();
                neighbours = _lldpSercives.GetNeighbours();
            }
            catch(Exception e)
            {
                viewModel.Error = e.Message;
            }

            var ethernetInterfaces = _ethernetServices
                .GetEthernetInterfaces()
                .ToList();

            viewModel.Interfaces = ethernetInterfaces
                    .Select(@interface => new InterfaceViewModel
                    {
                        AllowTagging = settings.AllowTagging,
                        IsActive = @interface.Status == OperationalStatus.Up,
                        Status = @interface.Status,
                        IsHostInterface = @interface.IsHostInterface,
                        Name = @interface.Name,
                        Tagged = @interface.Tagged,
                        Type = @interface.Type,
                        VirtualLANs = @interface.VirtualLANs,
                        Hidden = settings.HiddenInterfaces?.Contains(@interface.Name) ?? false,
                        Neighbor = neighbours.FirstOrDefault(x => x.EthernetInterfaceName == @interface.Name)
                    }).ToList();

            viewModel.VLANs = ethernetInterfaces
                .SelectMany(@interface => @interface.VirtualLANs)
                .Where(vLan => vLan.IsNotEmpty())
                .Distinct()
                .ToList()
                .Select(vLan => new BridgeViewModel
                {
                    Name = vLan, 
                    IpAddress = _ethernetServices.GetBridgeAddress(vLan),//_bash.Execu($"ip get for vlan{vLan}"), // todo get if of bridge
                    Interfaces = ethernetInterfaces
                        .Where(@interface => @interface.VirtualLANs.Contains(vLan))
                        .Select(@interface => @interface.Name)
                        .ToArray()
                });


            return View (viewModel);
        }

        /// <summary>
        ///     Action after "Update" button click
        /// </summary>
        /// <param name="viewModel">Interface options from form.</param>
        /// <returns>Redirect to home page</returns>
        public async Task<IActionResult> Edit (InterfaceViewModel viewModel)
        {
            var viewModelIsGvrp = viewModel.IsGVRP;

            switch (viewModel.Type)
            {
                case InterfaceType.Promiscuous when viewModel.VirtualLANs.Count() > 1:
                    return  RedirectToAction("Index", new IndexViewModel { Error = "Promiscuous Interface cannot have more than 1 VLAN." });
                case InterfaceType.Isolated when viewModel.VirtualLANs.Count() > 1:
                    return RedirectToAction("Index", new IndexViewModel { Error = "Isolated Interface cannot have more than 1 VLAN." });
            }

            _ethernetServices.SetEthernetInterfaceState(viewModel.Name, viewModel.IsActive);
            _ethernetServices.ClearEthernetInterfaceVLANs(viewModel.Name);
            _ethernetServices.ApplyEthernetInterfaceVLANs(viewModel.Name, viewModel.Tagged, viewModel.VirtualLANs);
            _ethernetServices.SetEthernetInterfaceType(viewModel.Name, viewModel.Type);

            return RedirectToAction ("Index");
        }

        public async Task<IActionResult> EditBridge(BridgeViewModel viewModel)
        {
            _ethernetServices.SetBridgeAddress(viewModel.Name, viewModel.IpAddress, viewModel.Interfaces);
            return RedirectToAction("Index");
        }



        [ResponseCache (Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error () {
            return View (new ErrorViewModel {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}