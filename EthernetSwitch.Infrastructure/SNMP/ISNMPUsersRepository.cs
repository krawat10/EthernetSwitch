using EthernetSwitch.Data;
using EthernetSwitch.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EthernetSwitch.Infrastructure.SNMP
{
    public interface ISNMPUsersRepository
    {
        Task<ICollection<SNMPUser>> GetUsers();
        Task Add(SNMPUser user, CancellationToken token = default);
    }

    public class SNMPUsersRepository : ISNMPUsersRepository
    {
        private readonly EthernetSwitchContext context;

        public SNMPUsersRepository(EthernetSwitchContext context)
        {
            this.context = context;
        }
        public async Task Add(SNMPUser user, CancellationToken token = default)
        {
            await context.SNMPUsers.AddAsync(user, token);
            await context.SaveChangesAsync(token);
        }

        public async Task<ICollection<SNMPUser>> GetUsers()
        {
            return await context.SNMPUsers.ToListAsync();
        }
    }
}
