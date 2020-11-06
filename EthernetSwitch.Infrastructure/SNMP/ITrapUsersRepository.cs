using EthernetSwitch.Data;
using EthernetSwitch.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EthernetSwitch.Infrastructure.SNMP
{
    public interface ITrapUsersRepository
    {
        Task<ICollection<SNMPTrapUser>> GetUsers();
        Task AddUser(SNMPTrapUser user, CancellationToken token = default);
        Task<bool> HasNewUsers(ICollection<SNMPTrapUser> oldUsers, CancellationToken token = default);
        Task Remove(long id);
    }

    public class TrapUsersRepository : ITrapUsersRepository
    {
        private readonly EthernetSwitchContext context;

        public TrapUsersRepository(EthernetSwitchContext context)
        {
            this.context = context;
        }
        public async Task AddUser(SNMPTrapUser user, CancellationToken token = default)
        {
            await context.AddAsync(user);
            await context.SaveChangesAsync(token);
        }

        public async Task<bool> HasNewUsers(ICollection<SNMPTrapUser> oldUsers, CancellationToken token = default)
        {
            return !(await context.TrapUsers.Select(x => x.Id).ToListAsync())
                .SequenceEqual(oldUsers.Select(x => x.Id));
        }

        public async Task<ICollection<SNMPTrapUser>> GetUsers()
        {
            return await context.TrapUsers.ToListAsync();
        }

        public async Task Remove(long id)
        {
            context.TrapUsers.Remove(await context.TrapUsers.FindAsync(id));
            await context.SaveChangesAsync();
        }
    }
}
