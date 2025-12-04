using System;
using OtpNet;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Providers;
using PayPulse.Domain.Interfaces.Repositories;
using PayPulse.Domain.Interfaces.Services;
using PayPulse.Domain.Settings;

namespace PayPulse.Core.Services
{
    public class ProfileAuthService
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IAuthProviderClient _authClient;
        private readonly IDateTimeProvider _clock;
        private readonly LoggerService _logger;

        public ProfileAuthService(
            ISettingsRepository settingsRepository,
            IProfileRepository profileRepository,
            IAuthProviderClient authClient,
            IDateTimeProvider clock,
            LoggerService logger)
        {
            _settingsRepository = settingsRepository;
            _profileRepository = profileRepository;
            _authClient = authClient;
            _clock = clock;
            _logger = logger;
        }

        public string EnsureToken(Profile profile, out bool needsCredentials)
        {
            needsCredentials = false;

            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }

            if (!string.IsNullOrWhiteSpace(profile.ManualToken))
            {
                return profile.ManualToken;
            }

            if (!string.IsNullOrWhiteSpace(profile.TotpSecret) &&
                !string.IsNullOrWhiteSpace(profile.LoginPassword) &&
                (!string.IsNullOrWhiteSpace(profile.LoginEmail) ||
                 !string.IsNullOrWhiteSpace(profile.LoginPhoneNumber)))
            {
                try
                {
                    var settings = _settingsRepository.Load() ?? new AppSettings();
                    ProviderSettings providerSettings = null;
                    if (settings.Providers != null)
                    {
                        providerSettings = settings.Providers.Get(profile.ProviderType);
                    }
                    var baseUrl = providerSettings != null && !string.IsNullOrWhiteSpace(providerSettings.BaseUrl)
                        ? providerSettings.BaseUrl
                        : settings.StbApi.BaseUrl;

                    string otp = GenerateTotp(profile.TotpSecret);

                    string token;
                    if (!string.IsNullOrWhiteSpace(profile.LoginEmail))
                    {
                        token = _authClient.LoginWithEmail(
                            baseUrl,
                            profile.LoginEmail,
                            profile.LoginPassword,
                            otp);
                    }
                    else
                    {
                        token = _authClient.LoginWithPhone(
                            baseUrl,
                            profile.LoginPhoneNumber,
                            profile.LoginPassword,
                            otp);
                    }

                    profile.ManualToken = token;
                    profile.LastSyncUtc = _clock.UtcNow;
                    _profileRepository.Update(profile);

                    _logger.Info("Auto-login succeeded for profile " + profile.Name, null, profile.ProfileId, "AUTH");
                    return token;
                }
                catch (Exception ex)
                {
                    _logger.Error("Auto-login failed for profile " + profile.Name, ex, null, profile.ProfileId, "AUTH");
                }
            }

            needsCredentials = true;
            return null;
        }

        private string GenerateTotp(string base32Secret)
        {
            string clean = base32Secret.Replace(" ", string.Empty);
            byte[] bytes = Base32Encoding.ToBytes(clean);
            var totp = new Totp(bytes);
            return totp.ComputeTotp();
        }

        public string LoginWithUserProvidedOtp(Profile profile, string otp)
        {
var settings = _settingsRepository.Load() ?? new AppSettings();
ProviderSettings providerSettings = null;
if (settings.Providers != null)
{
    providerSettings = settings.Providers.Get(profile.ProviderType);
}
var baseUrl = providerSettings != null && !string.IsNullOrWhiteSpace(providerSettings.BaseUrl)
    ? providerSettings.BaseUrl
    : settings.StbApi.BaseUrl;

            if (!string.IsNullOrWhiteSpace(profile.LoginEmail))
            {
                profile.ManualToken = _authClient.LoginWithEmail(
                    baseUrl,
                    profile.LoginEmail,
                    profile.LoginPassword,
                    otp);
            }
            else
            {
                profile.ManualToken = _authClient.LoginWithPhone(
                    baseUrl,
                    profile.LoginPhoneNumber,
                    profile.LoginPassword,
                    otp);
            }

            profile.LastSyncUtc = _clock.UtcNow;
            _profileRepository.Update(profile);

            _logger.Info("Manual login succeeded for profile " + profile.Name, null, profile.ProfileId, "AUTH");
            return profile.ManualToken;
        }
    }
}
