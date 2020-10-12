using System.Linq;
using System.Threading.Tasks;
using EthernetSwitch.Data;
using EthernetSwitch.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EthernetSwitch.Infrastructure
{
    class SettingsRepository : ISettingsRepository
    {
        private readonly EthernetSwitchContext _context;

        public SettingsRepository(EthernetSwitchContext context)
        {
            _context = context;

            if (!context.Settings.Any())
            {
                context.Settings.Add(new Settings
                {
                    AllowRegistration = false,
                    AllowTagging = false,
                    RequireConfirmation = true
                });

                context.SaveChanges();
            }
        }

        public async Task<Settings> GetSettings()
        {
            return await _context.Settings.FirstOrDefaultAsync();
        }

        public async Task SaveSettings(Settings settings)
        {
            _context.Settings.Update(settings);
            await _context.SaveChangesAsync();
        }
    }
}