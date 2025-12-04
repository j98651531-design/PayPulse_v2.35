using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PayPulse.Core.DTOs;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Repositories;
using PayPulse.Domain.Interfaces.Services;

namespace PayPulse.Core.Services
{
    public class BackgroundSyncService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly ProfileAuthService _profileAuthService;
        private readonly TransferFetchService _fetchService;
        private readonly TransferNormalizationService _normalizeService;
        private readonly AddTransfersToPosService _addToPosService;
        private readonly IDateTimeProvider _clock;
        private readonly LoggerService _logger;

        private CancellationTokenSource _cts;

        public BackgroundSyncService(
            IProfileRepository profileRepository,
            ProfileAuthService profileAuthService,
            TransferFetchService fetchService,
            TransferNormalizationService normalizeService,
            AddTransfersToPosService addToPosService,
            IDateTimeProvider clock,
            LoggerService logger)
        {
            _profileRepository = profileRepository;
            _profileAuthService = profileAuthService;
            _fetchService = fetchService;
            _normalizeService = normalizeService;
            _addToPosService = addToPosService;
            _clock = clock;
            _logger = logger;
        }

        public void Start()
        {
            if (_cts != null)
            {
                return;
            }

            _cts = new CancellationTokenSource();
            _ = Task.Run(() => Loop(_cts.Token));
            _logger.Info("Background sync started", null, null, "BG");
        }

        public void Stop()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts = null;
                _logger.Info("Background sync stopped", null, null, "BG");
            }
        }

        private async Task Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    DoWorkOnce();
                }
                catch (Exception ex)
                {
                    _logger.Error("Background sync loop error", ex, null, null, "BG");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), token);
            }
        }

        private void DoWorkOnce()
        {
            var profiles = _profileRepository.GetAll();
            foreach (Profile profile in profiles)
            {
                if (!profile.IsActive)
                {
                    continue;
                }

                string correlationId = Guid.NewGuid().ToString("N");
                try
                {
                    bool needsCredentials;
                    string token = _profileAuthService.EnsureToken(profile, out needsCredentials);
                    if (needsCredentials || string.IsNullOrWhiteSpace(token))
                    {
                        _logger.Warn("Background sync: profile " + profile.Name + " needs credentials, skipping",
                            correlationId, profile.ProfileId, "BG");
                        continue;
                    }

                    DateTime today = _clock.Today;
                    DateTime start = today.AddDays(-1);
                    DateTime end = today.AddDays(1).AddSeconds(-1);

                    var rangeDto = new DateRangeRequestDto { Start = start, End = end };

                    var swTotal = Stopwatch.StartNew();
                    var swFetch = Stopwatch.StartNew();

                    _logger.Info("BG fetch for profile " + profile.Name, correlationId, profile.ProfileId, "BG/FETCH");
                    _fetchService.FetchForProfile(rangeDto, profile, token, correlationId);

                    swFetch.Stop();
                    var swNormalize = Stopwatch.StartNew();

                    _logger.Info("BG normalize for profile " + profile.Name, correlationId, profile.ProfileId, "BG/NORMALIZE");
                    _normalizeService.NormalizeForProfile(rangeDto, profile, token, correlationId);

                    swNormalize.Stop();
                    var swAddToPos = Stopwatch.StartNew();

                    _logger.Info("BG add-to-POS for profile " + profile.Name, correlationId, profile.ProfileId, "BG/ADDPOS");
                    _addToPosService.AddPendingToPos(profile, profile.PosUserId, profile.PosCashboxId, correlationId);

                    swAddToPos.Stop();
                    swTotal.Stop();

                    var perfMessage = string.Format(
                        "BG perf: fetchMs={0}; normalizeMs={1}; addToPosMs={2}; totalMs={3}",
                        swFetch.ElapsedMilliseconds,
                        swNormalize.ElapsedMilliseconds,
                        swAddToPos.ElapsedMilliseconds,
                        swTotal.ElapsedMilliseconds);

                    _logger.Info(perfMessage, correlationId, profile.ProfileId, "BG/PERF");
                }
                catch (Exception ex)
                {
                    _logger.Error("Background sync error for profile " + profile.Name, ex, correlationId, profile.ProfileId, "BG");
                }
            }
        }
    }
}
