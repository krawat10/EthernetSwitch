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
        Task<ICollection<TrapUser>> GetTrapUsers();
        Task AddTrapUser(TrapUser user, CancellationToken token = default);
        Task<bool> HasNewUsers(ICollection<TrapUser> oldUsers, CancellationToken token = default);
    }

    public class TrapUsersRepository : ITrapUsersRepository
    {
        private readonly EthernetSwitchContext context;

        public TrapUsersRepository(EthernetSwitchContext context)
        {
            this.context = context;
        }
        public async Task AddTrapUser(TrapUser user, CancellationToken token = default)
        {
            await context.AddAsync(user);
            await context.SaveChangesAsync(token);
        }

        public async Task<bool> HasNewUsers(ICollection<TrapUser> oldUsers, CancellationToken token = default)
        {
            return !(await context.TrapUsers.Select(x => x.Id).ToListAsync())
                .SequenceEqual(oldUsers.Select(x => x.Id));
        }

        public async Task<ICollection<TrapUser>> GetTrapUsers()
        {
            return await context.TrapUsers.ToListAsync();
        }
    }
}
