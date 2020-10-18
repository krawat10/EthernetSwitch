using System.Threading.Tasks;

namespace EthernetSwitch.Infrastructure.Settings
{
    public interface ISettingsRepository
    {
        Task<Data.Models.Settings> GetSettings();
        Task SaveSettings(Data.Models.Settings settings);
    }
}