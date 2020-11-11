using System.Threading.Tasks;
using EthernetSwitch.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace EthernetSwitch.Infrastructure.Settings
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly EthernetSwitchContext _context;

        public SettingsRepository(EthernetSwitchContext context)
        {
            _context = context;

            if (!context.Settings.Any())
            {
                context.Settings.Add(new Data.Models.Settings
                {
                    AllowRegistration = false,
                    AllowTagging = false,
                    RequireConfirmation = true,
                    SNMPConfiguration = new Data.Models.SNMPConfiguration()
                });

                context.SaveChanges();
            }
        }

        public async Task<Data.Models.Settings> GetSettings()
        {
            return await _context.Settings.FirstOrDefaultAsync();
        }

        public async Task SaveSettings(Data.Models.Settings settings)
        {
            _context.Settings.Update(settings);
            await _context.SaveChangesAsync();
        }
    }
}