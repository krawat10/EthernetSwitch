using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EthernetSwitch.Models;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace EthernetSwitch.Infrastructure
{
    public interface IUserService
    {
        Task<User> Login(string username, string password);
        Task<User> Register(string username, string password, UserRole role = UserRole.NotConfirmed);
        Task<User> ChangePassword(string username, string password);
        Task RegisterUsers(string[] userNames);
        Task RemoveUsers(string[] userNames);
    }

    public class UserService : IUserService
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly PasswordHasher<string> _passwordHasher;

        public UserService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
            _passwordHasher = new PasswordHasher<string>();
        }



        public async Task<User> Login(string username, string password)
        {
            var settings =  _settingsRepository.GetSettings();
            var user = settings.Users.FirstOrDefault(usr => usr.UserName == username);

            if (user == null) throw new ArgumentException("Wrong password or username");

            var result = _passwordHasher.VerifyHashedPassword(user.UserName, user.PasswordEncrypted, password);

            return result == PasswordVerificationResult.Success ? user : null;
        }

        public async Task<User> Register(string username, string password, UserRole role = UserRole.NotConfirmed)
        {
            var user = new User
            {
                Role = role,
                UserName = username,
                PasswordEncrypted = _passwordHasher.HashPassword(username, password)
            };

            var settings =  _settingsRepository.GetSettings();

            settings.Users.Append(user);

            _settingsRepository.SaveSettings(settings);

            return user;
        }

        public async Task<User> ChangePassword(string username, string password)
        {
            var settings =  _settingsRepository.GetSettings();

            var user = settings.Users.FirstOrDefault(usr => usr.UserName == username);

            if (user == null) throw new ArgumentException($"User {user} does not exists");

            user.PasswordEncrypted = _passwordHasher.HashPassword(username, password);

            _settingsRepository.SaveSettings(settings);

            return user;
        }

        public async Task RegisterUsers(string[] userNames)
        {
            var settings =  _settingsRepository.GetSettings();

            foreach (var userName in userNames)
            {
                var user = settings.Users.FirstOrDefault(user1 => user1.UserName == userName);

                if (user != null && user.Role != UserRole.Admin)
                {
                    user.Role = UserRole.User;
                }
            }

            _settingsRepository.SaveSettings(settings);
        }

        public async Task RemoveUsers(string[] userNames)
        {
            var settings =  _settingsRepository.GetSettings();
            var users = settings.Users.ToList();


            settings.Users = users.Where(user => !userNames.Contains(user.UserName));

            _settingsRepository.SaveSettings(settings);
        }
    }
}