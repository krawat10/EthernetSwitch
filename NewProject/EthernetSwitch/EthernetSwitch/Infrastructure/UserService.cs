using System;
using System.IO;
using System.Linq;
using EthernetSwitch.Models;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace EthernetSwitch.Infrastructure
{
    public interface IUserService
    {
        User Login(string username, string password);
        User Register(string username, string password);
        User ChangePassword(string username, string password);
    }

    public class UserService : IUserService
    {
        private readonly IHostEnvironment _environment;
        private readonly PasswordHasher<string> _passwordHasher;
        private readonly string _filename;

        public UserService(IHostEnvironment environment)
        {
            _environment = environment;
            _passwordHasher = new PasswordHasher<string>();
            _filename = "settings.json";
        }

        private Settings GetSettings()
        {
            return JsonSerializer.Deserialize<Settings>(File.ReadAllText(_filename));
        }

        private void SaveSettings(Settings settings)
        {
            File.WriteAllText(_filename, JsonSerializer.Serialize(settings));
        }

        public User Login(string username, string password)
        {
            var user = GetSettings().Users.FirstOrDefault(usr => usr.UserName == username);

            if (user == null) throw new ArgumentException("Wrong password or username");

            var result = _passwordHasher.VerifyHashedPassword(user.UserName, user.PasswordEncrypted, password);

            return result == PasswordVerificationResult.Success ? user : null;
        }

        public User Register(string username, string password)
        {
            var user = new User
            {
                CanEdit = true,
                IsAdmin = false,
                UserName = username,
                PasswordEncrypted = _passwordHasher.HashPassword(username, password)
            };

            var settings = GetSettings();

            settings.Users.Append(user);

            SaveSettings(settings);

            return user;
        }

        public User ChangePassword(string username, string password)
        {
            var settings = GetSettings();

            var user = settings.Users.FirstOrDefault(usr => usr.UserName == username);

            if(user == null) throw new ArgumentException($"User {user} does not exists");

            user.PasswordEncrypted = _passwordHasher.HashPassword(username, password);

            SaveSettings(settings);

            return user;
        }
    }
}