using EthernetSwitch.Data;
using EthernetSwitch.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EthernetSwitch.Infrastructure.SNMP
{
    public interface ISNMPMessageStore
    {
        Task<IEnumerable<SNMPMessage>> GetAll();
        Task RemoveAll();
        Task Add(SNMPMessage message);
    }

    public class SNMPMessageStore : ISNMPMessageStore
    {
        private readonly EthernetSwitchContext _context;

        public SNMPMessageStore(EthernetSwitchContext context)
        {
            _context = context;
        }
        public async Task Add(SNMPMessage message)
        {
            await _context.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SNMPMessage>> GetAll()
        {
            return await _context.TrapMessages
                            .OrderByDescending(x => x.TimeStamp)
                            .Include(x => x.Variables)
                            .ToListAsync();
        }

        public async Task RemoveAll()
        {
            _context.TrapMessages.RemoveRange(_context.TrapMessages);
            await _context.SaveChangesAsync();
        }
    }
}
