using EthernetSwitch.Models;
using Microsoft.Extensions.Hosting;

namespace EthernetSwitch.Infrastructure
{
    public interface ISettingsRepository
    {
        Settings GetSettings();
        void SaveSettings(Settings settings);
    }
}