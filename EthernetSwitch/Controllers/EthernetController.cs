using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using EthernetSwitch.Infrastructure.Ethernet;
using EthernetSwitch.Infrastructure.Settings;
using EthernetSwitch.Models;
using EthernetSwitch.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        public async Task<IActionResult> Index () {
            var settings = await _settingsRepository.GetSettings ();
            var neighbours = _lldpSercives.GetNeighbours();

            var viewModel = new IndexViewModel
            {
                Interfaces = _ethernetServices
                    .GetEthernetInterfaces()
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
                        Hidden = settings.HiddenInterfaces.Contains(@interface.Name),
                        Neighbor = neighbours.FirstOrDefault(x => x.EthernetInterfaceName == @interface.Name)
                    })
            };

            return View (viewModel);
        }

        /// <summary>
        ///     Action after "Update" button click
        /// </summary>
        /// <param name="viewModel">Interface options from form.</param>
        /// <returns>Redirect to home page</returns>
        public IActionResult Edit (InterfaceViewModel viewModel) {
                
            _ethernetServices.SetEthernetInterfaceState(viewModel.Name, viewModel.IsActive);
            _ethernetServices.ClearEthernetInterfaceVLANs(viewModel.Name);
            _ethernetServices.ApplyEthernetInterfaceVLANs(viewModel.Name, viewModel.VirtualLANs);
            _ethernetServices.SetEthernetInterfaceType(viewModel.Name, viewModel.Type);

            return RedirectToAction ("Index");
        }



        [ResponseCache (Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error () {
            return View (new ErrorViewModel {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}