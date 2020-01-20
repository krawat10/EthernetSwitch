using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using EthernetSwitch.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace EthernetSwitch.Controllers
{
    [Route("User")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View("Login", new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> LoginPost(LoginViewModel model, string ReturnUrl)
        {
            ReturnUrl = ReturnUrl ?? Url.Content("~/");
            var user = _userService.Login(model.UserName, model.Password);
            if (user == null)
            {
                model.Password = String.Empty;
                model.Message = "Wrong password or username";

                return View("Login", model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin":"User"),
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

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }
    }
}