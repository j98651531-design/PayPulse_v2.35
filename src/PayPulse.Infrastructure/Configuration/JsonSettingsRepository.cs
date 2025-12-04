using System;
using System.IO;
using Newtonsoft.Json;
using PayPulse.Domain.Interfaces.Repositories;
using PayPulse.Domain.Settings;

namespace PayPulse.Infrastructure.Configuration
{
    public class JsonSettingsRepository : ISettingsRepository
    {
        private readonly string _path;

        public JsonSettingsRepository(string path)
        {
            _path = path;
        }

        public AppSettings Load()
        {
            if (!File.Exists(_path))
            {
                return new AppSettings();
            }

            string json = File.ReadAllText(_path);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new AppSettings();
            }

            try
            {
                AppSettings s = JsonConvert.DeserializeObject<AppSettings>(json);
                return s ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void Save(AppSettings settings)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(_path, json);
        }
    }
}
