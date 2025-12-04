using System;
using System.IO;
using System.Windows.Forms;
using PayPulse.Core.Services;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Providers;
using PayPulse.Domain.Interfaces.Repositories;
using PayPulse.Domain.Interfaces.Services;
using PayPulse.Domain.Settings;
using PayPulse.Infrastructure.Configuration;
using PayPulse.Infrastructure.Persistence.Access;
using PayPulse.Infrastructure.Persistence.SQLite;
using PayPulse.Infrastructure.Providers;
using PayPulse.Infrastructure.Security;
using PayPulse.Infrastructure.Time;
using PayPulse.Core.Billing;
using PayPulse.Infrastructure.Billing;

namespace PayPulse.WinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string appSettingsPath = Path.Combine(appDir, "appsettings.json");

            ISettingsRepository settingsRepo = new JsonSettingsRepository(appSettingsPath);
            var settingsService = new SettingsService(settingsRepo);
            var settings = settingsService.Load() ?? new AppSettings();

            string syncDbPath = Path.Combine(
                appDir,
                string.IsNullOrWhiteSpace(settings.Databases.SyncDbPath)
                    ? "sync.db"
                    : settings.Databases.SyncDbPath);

            string posDbPath = Path.Combine(
                appDir,
                string.IsNullOrWhiteSpace(settings.Databases.PosDbPath)
                    ? "pos.mdb"
                    : settings.Databases.PosDbPath);

            var sqliteFactory = new SqliteConnectionFactory(syncDbPath);

            IPosCustomerRepository posCustomerRepo;
            IPosOperationRepository posOperationRepo;
            IPosMetadataRepository posMetadataRepo;

            if (settings.Databases.PosDbType == PosDbType.FoxPro)
            {
                var foxFolder = string.IsNullOrWhiteSpace(settings.Databases.PosFoxProFolder)
                    ? appDir
                    : settings.Databases.PosFoxProFolder;

                var foxFactory = new PayPulse.Infrastructure.Persistence.FoxPro.FoxProConnectionFactory(foxFolder);
                posCustomerRepo = new PayPulse.Infrastructure.Persistence.FoxPro.PosCustomerRepository(foxFactory);
                posOperationRepo = new PayPulse.Infrastructure.Persistence.FoxPro.PosOperationRepository(foxFactory);
                posMetadataRepo = new PayPulse.Infrastructure.Persistence.FoxPro.PosMetadataRepository(foxFactory);
            }
            else
            {
                posDbPath = Path.Combine(
                    appDir,
                    string.IsNullOrWhiteSpace(settings.Databases.PosDbPath)
                        ? "pos.mdb"
                        : settings.Databases.PosDbPath);

                var accessFactory = new AccessConnectionFactory(posDbPath);
                posCustomerRepo = new PosCustomerRepository(accessFactory);
                posOperationRepo = new PosOperationRepository(accessFactory);
                posMetadataRepo = new PosMetadataRepository(accessFactory);
            }

            ITransfersRepository transfersRepo = new TransfersRepository(sqliteFactory);
            ILogRepository logRepo = new LogRepository(sqliteFactory);
            IProfileRepository profileRepo = new ProfileRepository(sqliteFactory);
            IAppUserRepository appUserRepo = new AppUserRepository(sqliteFactory);

            IDateTimeProvider clock = new SystemDateTimeProvider();
            ITokenDecoder tokenDecoder = new JwtTokenDecoder();
            var currentUserContext = new CurrentUserContext();
            var logger = new LoggerService(logRepo, clock, currentUserContext);

            IProviderClient providerClient = new StbProviderClient();
            ICustomerProviderClient customerProviderClient = (ICustomerProviderClient)providerClient;
            IAuthProviderClient authProviderClient = (IAuthProviderClient)providerClient;

            var posMetadataService = new PosMetadataService(posMetadataRepo);

                        // Billing configuration
            IBillingRepository billingRepository = new BillingRepository(syncDbPath);
            var billingTariffs = billingRepository.GetTariffs() ?? new BillingTariffs
            {
                PricePerTransfer = 0.02m,
                PricePerAddToPos = 0.03m,
                PricePerCustomer = 0.01m,
                Currency = "USD"
            };
            IBillingEngine billingEngine = new BillingEngine(billingRepository, billingTariffs);

var fetchService = new TransferFetchService(
                settingsRepo,
                transfersRepo,
                providerClient,
                tokenDecoder,
                clock,
                logger,
                billingEngine);

            var normalizeService = new TransferNormalizationService(
                settingsRepo,
                transfersRepo,
                posCustomerRepo,
                customerProviderClient,
                tokenDecoder,
                logger,
                billingEngine);

            var addToPosService = new AddTransfersToPosService(
                settingsRepo,
                transfersRepo,
                posCustomerRepo,
                posOperationRepo,
                logger,
                billingEngine);

            var reportsService = new ReportsQueryService(
                settingsRepo,
                transfersRepo,
                tokenDecoder);

            var profileAuthService = new ProfileAuthService(
                settingsRepo,
                profileRepo,
                authProviderClient,
                clock,
                logger);

            var bgService = new BackgroundSyncService(
                profileRepo,
                profileAuthService,
                fetchService,
                normalizeService,
                addToPosService,
                clock,
                logger);

            

            // Login first
            using (var loginForm = new LoginForm(appUserRepo, currentUserContext))
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }

            bgService.Start();

            var mainForm = new MainForm(
                settingsService,
                fetchService,
                normalizeService,
                addToPosService,
                reportsService,
                posMetadataService,
                profileRepo,
                bgService,
                logger,
                logRepo,
                profileAuthService,
                appUserRepo,
                currentUserContext,
                billingEngine,
                billingRepository);

            Application.Run(mainForm);

        }
    }
}
