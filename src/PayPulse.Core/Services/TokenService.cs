using PayPulse.Domain.Interfaces.Repositories;
using PayPulse.Domain.Settings;

namespace PayPulse.Core.Services
{
    public class TokenService
    {
        private readonly ISettingsRepository _settingsRepository;

        public TokenService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public string GetGlobalToken()
        {
            var s = _settingsRepository.Load() ?? new AppSettings();
            return s.Token.JwtToken;
        }

        public void SaveGlobalToken(string token)
        {
            var s = _settingsRepository.Load() ?? new AppSettings();
            s.Token.JwtToken = token;
            _settingsRepository.Save(s);
        }
    }
}
