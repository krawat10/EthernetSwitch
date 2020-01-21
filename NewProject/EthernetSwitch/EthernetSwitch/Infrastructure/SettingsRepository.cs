using System.IO;
using System.Text.Json;
using EthernetSwitch.Models;
using Microsoft.Extensions.Configuration;

namespace EthernetSwitch.Infrastructure
{
    class SettingsRepository : ISettingsRepository
    {
        private string _filename;

        public SettingsRepository(IConfiguration configuration)
        {
            _filename = configuration["SettingsFile"];
        }

        public Settings GetSettings()
        {
            return JsonSerializer.Deserialize<Settings>(File.ReadAllText(_filename));
        }

        public void SaveSettings(Settings settings)
        {
            File.WriteAllText(_filename, JsonSerializer.Serialize(settings));
        }
    }
}