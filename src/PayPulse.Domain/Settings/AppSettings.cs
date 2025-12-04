using System;

namespace PayPulse.Domain.Settings
{
    public class AppSettings
    {
        // Legacy STB section kept for backward compatibility.
        public StbApiSettings StbApi { get; set; } = new StbApiSettings();

        // New multi-provider section (STB / GMT / WIC)
        public ProvidersSettings Providers { get; set; } = new ProvidersSettings();

        public DatabaseSettings Databases { get; set; } = new DatabaseSettings();
        public PosMappingsSettings PosMappings { get; set; } = new PosMappingsSettings();

        public bool AutoStartOnBoot { get; set; }

        public AutoUpdateSettings AutoUpdate { get; set; } = new AutoUpdateSettings();


        // Legacy global token section (no longer used in v2.7, but kept so old JSON still deserializes)
        public TokenSettings Token { get; set; } = new TokenSettings();
    }

    public class StbApiSettings
    {
        public string BaseUrl { get; set; }
    }

    public class ProvidersSettings
    {
        public ProviderSettings STB { get; set; } = new ProviderSettings();
        public ProviderSettings GMT { get; set; } = new ProviderSettings();
        public ProviderSettings WIC { get; set; } = new ProviderSettings();

        public ProviderSettings Get(string providerType)
        {
            if (string.IsNullOrWhiteSpace(providerType))
            {
                return STB;
            }

            switch (providerType.Trim().ToUpperInvariant())
            {
                case "STB":
                    return STB;
                case "GMT":
                    return GMT;
                case "WIC":
                    return WIC;
                default:
                    return STB;
            }
        }
    }

    public class ProviderSettings
    {
        public string BaseUrl { get; set; }
        public string AuthUrl { get; set; }
        public string TransfersUrl { get; set; }
        public string TransferDetailsUrl { get; set; }
        public string FindCustomerUrl { get; set; }
    }


    public enum PosDbType
    {
        Access = 0,
        FoxPro = 1
    }

    public class DatabaseSettings
    {
        public string SyncDbPath { get; set; }
        public string PosDbPath { get; set; }
        public string AppSettingsPath { get; set; }

        public PosDbType PosDbType { get; set; } = PosDbType.Access;

        // v2.7: Global POS type per workstation
        // "Changer"  => Access (.mdb / .accdb)
        // "ChangeMat" => Visual FoxPro (.dbf folder)
        public string PosType { get; set; }

        // Used when PosType == "ChangeMat"
        public string PosFoxProFolder { get; set; }
    }

    public class PosMappingsSettings
    {
        public string CurrencyUsdId { get; set; }
        public string CurrencyEurId { get; set; }
        public string CurrencyIlsId { get; set; }

        // Legacy global UserId / CashboxId retained for backward compatibility,
        // but v2.7 uses per-profile POS user/cashbox instead.
        public string UserId { get; set; }
        public string CashboxId { get; set; }

        public string IdTypeId { get; set; }
        public string IdTypePassport { get; set; }
        public string UserTypeCitizen { get; set; }
        public string UserTypeTourist { get; set; }
    }



    public class AutoUpdateSettings
    {
        public bool CheckOnStartup { get; set; } = true;
        public int CheckIntervalHours { get; set; } = 4;
        public DateTime? LastCheckUtc { get; set; }
    }

    public class TokenSettings
    {
        public string JwtToken { get; set; }
    }
}
