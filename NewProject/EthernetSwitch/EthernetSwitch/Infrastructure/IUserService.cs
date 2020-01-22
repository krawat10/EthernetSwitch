using System.Collections.Generic;
using EthernetSwitch.Models;

namespace EthernetSwitch.Infrastructure
{
    public interface IUserService
    {
        User Login(string username, string password);
        User Register(string username, string password, UserRole role = UserRole.NotConfirmed);
        User ChangePassword(string username, string password);
        void RegisterUsers(IEnumerable<string> userNames);
        void RemoveUsers(string[] userNames);
    }
}