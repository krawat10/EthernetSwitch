using System.Collections.Generic;
using System.Threading.Tasks;
using EthernetSwitch.Data.Models;

namespace EthernetSwitch.Infrastructure.Users
{
    public interface IUserService
    {
        Task<User> Login(string username, string password);
        Task<User> Register(string username, string password, UserRole role = UserRole.NotConfirmed);
        Task<IEnumerable<User>> GetUsers();
        Task<User> ChangePassword(string username, string password);
        Task RegisterUsers(IEnumerable<string> userNames);
        Task RemoveUsers(IEnumerable<string> userNames);
    }
}