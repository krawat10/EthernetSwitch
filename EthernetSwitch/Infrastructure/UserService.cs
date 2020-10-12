using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EthernetSwitch.Models;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Threading.Tasks;
using EthernetSwitch.Data;
using EthernetSwitch.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EthernetSwitch.Infrastructure
{
    public class UserService : IUserService
    {
        private readonly EthernetSwitchContext _context;
        private readonly PasswordHasher<string> _passwordHasher;

        public UserService(EthernetSwitchContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<string>();

            if (!_context.Users.Any())
            {
                _context.Users.Add(new User
                {
                    PasswordEncrypted = "AQAAAAEAACcQAAAAEGFy8Wkpl5tSDHaZ0Gb1k5ZfUL2vWNmGncAD199qZkhFgvsvN/D16BZI0kkgxal4vw==",
                    UserName = "admin",
                    Role = UserRole.Admin
                });
                _context.SaveChanges();
            }
        }

        public async Task<User> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(usr => usr.UserName == username);

            if (user == null) return null;

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


            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> ChangePassword(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(usr => usr.UserName == username);

            if (user == null) throw new ArgumentException($"User {user} does not exists");

            if (_passwordHasher.VerifyHashedPassword(username, user.PasswordEncrypted, password) ==
                PasswordVerificationResult.Failed)
            {
                throw new ArgumentException("Old password is incorrect");
            }

            user.PasswordEncrypted = _passwordHasher.HashPassword(username, password);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task RegisterUsers(IEnumerable<string> userNames)
        {
            foreach (var userName in userNames)
            {
                var user = _context.Users.FirstOrDefault(user1 => user1.UserName == userName);

                if (user != null && user.Role != UserRole.Admin)
                {
                    user.Role = UserRole.User;
                    _context.Users.Update(user);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveUsers(IEnumerable<string> userNames)
        {
            var users = _context.Users.ToList();

            foreach (var user in users)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
        }
    }
}