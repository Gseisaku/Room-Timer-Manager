using System;
using System.IO;
using System.Text.Json;
using MultiRoomTimer.Models;

namespace MultiRoomTimer.Services
{
    public class ConfigurationService
    {
        private const string ConfigFileName = "config.json";
        private readonly string _configPath;

        public ConfigurationService()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
        }

        public AppConfig LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var config = JsonSerializer.Deserialize<AppConfig>(json, options);
                    if (config != null)
                    {
                        return config;
                    }
                }
                else
                {
                    // If file doesn't exist, create it with default values
                    var defaultConfig = new AppConfig();
                    SaveConfig(defaultConfig);
                    return defaultConfig;
                }
            }
            catch (Exception)
            {
                // If there's an error reading or parsing the config, we'll return default values
                // without overwriting the potentially malformed file.
            }

            return new AppConfig();
        }

        public void SaveConfig(AppConfig config)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception)
            {
                // Handle or log error
            }
        }
    }
}
