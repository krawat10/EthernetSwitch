﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EthernetSwitch.Data.Models;
using EthernetSwitch.Infrastructure;
using EthernetSwitch.Infrastructure.Settings;
using EthernetSwitch.Infrastructure.Users;
using EthernetSwitch.Models;
using EthernetSwitch.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EthernetSwitch.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ISettingsRepository _settingsRepository;

        public UserController(IUserService userService, ISettingsRepository settingsRepository)
        {
            _userService = userService;
            _settingsRepository = settingsRepository;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            var settings =await _settingsRepository.GetSettings();

            return View("Login", new LoginViewModel {CanRegister = settings.AllowRegistration});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                ReturnUrl ??= Url.Content("~/");
                var settings = await _settingsRepository.GetSettings();

                if (model.Type == LoginType.Register)
                {
                    if (settings.AllowRegistration)
                    {
                        var role = settings.RequireConfirmation ? UserRole.NotConfirmed : UserRole.User;
                        await _userService.Register(model.UserName, model.Password, role);
                    }
                }

                var user = await _userService.Login(model.UserName, model.Password);

                if (user == null)
                {
                    model.Password = String.Empty;
                    model.Message = "Wrong password or username";

                    return View("Login", model);
                }

                if (settings.RequireConfirmation && user.Role == UserRole.NotConfirmed)
                {
                    model.Password = String.Empty;
                    model.Message = "Your account is created but you must be approved by admin.";

                    return View("Login", model);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                    IssuedUtc = DateTimeOffset.Now,
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                return Redirect(ReturnUrl);
            }

            return View(model);
        }

        public IActionResult ChangePassword()
        {
            return View("ChangePassword", new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var username = User?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
            
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(username))
            {
                try
                {
                    _userService.ChangePassword(username, model.OldPassword, model.NewPassword);

                    return RedirectToAction("Index","Ethernet");
                }
                catch (ArgumentException e)
                {
                    model.Message = e.Message;
                }
            }
            
            model.OldPassword = string.Empty;

            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult RegisterUsers([Bind("UserNames")]IEnumerable<string> userNames)
        {
            _userService.RegisterUsers(userNames);

            return RedirectToAction("Index", "Settings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult RemoveUsers([Bind("UserNames")]IEnumerable<string> userNames)
        {
            _userService.RemoveUsers(userNames);


            return RedirectToAction("Index", "Settings");
        }
    }
}