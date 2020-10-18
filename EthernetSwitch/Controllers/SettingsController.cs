using System.Linq;
using System.Threading.Tasks;
using EthernetSwitch.Data.Models;
using EthernetSwitch.Infrastructure.Settings;
using EthernetSwitch.Infrastructure.Users;
using EthernetSwitch.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EthernetSwitch.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly IUserService _userService;

        public SettingsController(ISettingsRepository settingsRepository, IUserService userService)
        {
            _settingsRepository = settingsRepository;
            _userService = userService;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            var settings = await _settingsRepository.GetSettings();
            var users = await _userService.GetUsers();

            var model = new SettingsViewModel
            {
                AllowRegistration = settings.AllowRegistration,
                AllowTagging = settings.AllowTagging,
                RequireConfirmation = settings.RequireConfirmation,
                NotConfirmedUsers = users
                    .Where(user => user.Role == UserRole.NotConfirmed)
                    .Select(user => user.UserName),
                AllUsers = users
                    .Where(user => user.Role != UserRole.Admin)
                    .Select(user => user.UserName)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Post(SettingsViewModel model)
        {
            var settings = await _settingsRepository.GetSettings();

            if (ModelState.IsValid)
            {
                settings.AllowRegistration = model.AllowRegistration;
                settings.AllowTagging = model.AllowTagging;
                settings.RequireConfirmation = model.RequireConfirmation;

                await _settingsRepository.SaveSettings(settings);

                return RedirectToAction("Index", settings);
            }

            return View(model);
        }
    }
}