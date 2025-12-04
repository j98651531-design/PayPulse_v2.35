using PayPulse.Domain.Interfaces.Repositories;
using PayPulse.Domain.Settings;

namespace PayPulse.Core.Services
{
    public class SettingsService
    {
        private readonly ISettingsRepository _repo;

        public SettingsService(ISettingsRepository repo)
        {
            _repo = repo;
        }

        public AppSettings Load()
        {
            return _repo.Load();
        }

        public void Save(AppSettings s)
        {
            _repo.Save(s);
        }
    }
}
