using System.Threading.Tasks;
using EthernetSwitch.Data.Models;
using EthernetSwitch.Models;
using Microsoft.Extensions.Hosting;

namespace EthernetSwitch.Infrastructure
{
    public interface ISettingsRepository
    {
        Task<Settings> GetSettings();
        Task SaveSettings(Settings settings);
    }
}