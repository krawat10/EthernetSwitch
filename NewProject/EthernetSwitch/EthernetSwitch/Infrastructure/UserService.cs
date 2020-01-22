using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EthernetSwitch.Models;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EthernetSwitch.Infrastructure
{
    public class UserService : IUserService
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly PasswordHasher<string> _passwordHasher;

        public UserService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
            _passwordHasher = new PasswordHasher<string>();
        }



        public User Login(string username, string password)
        {
            var settings =  _settingsRepository.GetSettings();
            var user = settings.Users.FirstOrDefault(usr => usr.UserName == username);

            if (user == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(user.UserName, user.PasswordEncrypted, password);

            return result == PasswordVerificationResult.Success ? user : null;
        }

        public User Register(string username, string password, UserRole role = UserRole.NotConfirmed)
        {
            var user = new User
            {
                Role = role,
                UserName = username,
                PasswordEncrypted = _passwordHasher.HashPassword(username, password)
            };

            var settings =  _settingsRepository.GetSettings();

            settings.Users.Add(user);

            _settingsRepository.SaveSettings(settings);

            return user;
        }

        public User ChangePassword(string username, string password)
        {
            var settings =  _settingsRepository.GetSettings();

            var user = settings.Users.FirstOrDefault(usr => usr.UserName == username);

            if (user == null) throw new ArgumentException($"User {user} does not exists");

            if (_passwordHasher.VerifyHashedPassword(username, user.PasswordEncrypted, password) ==
                PasswordVerificationResult.Failed)
            {
                throw new ArgumentException("Old password is incorrect");
            }

            user.PasswordEncrypted = _passwordHasher.HashPassword(username, password);

            _settingsRepository.SaveSettings(settings);

            return user;
        }

        public void RegisterUsers(IEnumerable<string> userNames)
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

        public void RemoveUsers(IEnumerable<string> userNames)
        {
            var settings =  _settingsRepository.GetSettings();
            var users = settings.Users.ToList();


            settings.Users = users.Where(user => !userNames.Contains(user.UserName)).ToList();

            _settingsRepository.SaveSettings(settings);
        }
    }
}