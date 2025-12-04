using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using PayPulse.Core.DTOs;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Repositories;
using PayPulse.Domain.Interfaces.Services;

namespace PayPulse.Core.Services
{
    public class ProfileSyncDashboardService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly ILogRepository _logRepository;
        private readonly IDateTimeProvider _clock;

        public ProfileSyncDashboardService(
            IProfileRepository profileRepository,
            ILogRepository logRepository,
            IDateTimeProvider clock)
        {
            _profileRepository = profileRepository;
            _logRepository = logRepository;
            _clock = clock;
        }

        /// <summary>
        /// Builds a snapshot of per-profile sync statistics for the given lookback window.
        /// </summary>
        /// <param name="lookbackDays">How many days back to scan logs for activity.</param>
        public IList<ProfileSyncStatsDto> GetSnapshot(int lookbackDays)
        {
            if (lookbackDays <= 0)
            {
                lookbackDays = 7;
            }

            var now = _clock.UtcNow;
            var from = now.AddDays(-lookbackDays);

            var profiles = _profileRepository.GetAll();
            var logs = _logRepository.Get(from, now);

            var byProfile = logs
                .Where(l => !string.IsNullOrWhiteSpace(l.ProfileId))
                .GroupBy(l => l.ProfileId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<ProfileSyncStatsDto>();

            foreach (var profile in profiles)
            {
                byProfile.TryGetValue(profile.ProfileId, out var plist);
                plist = plist ?? new List<LogEntry>();

                var infoLogs = plist
                    .Where(l => string.Equals(l.Level, "INFO", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var errorLogs = plist
                    .Where(l => string.Equals(l.Level, "ERROR", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Background phase counts
                var bgFetch = infoLogs.Where(l =>
                        string.Equals(l.Operation, "BG/FETCH", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var bgNormalize = infoLogs.Where(l =>
                        string.Equals(l.Operation, "BG/NORMALIZE", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var bgAddToPos = infoLogs.Where(l =>
                        string.Equals(l.Operation, "BG/ADDPOS", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var bgErrors = errorLogs.Where(l =>
                        string.Equals(l.Operation, "BG", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Detailed phase errors
                var normalizeErrors = errorLogs.Where(l =>
                        string.Equals(l.Operation, "NORMALIZE", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var addToPosErrors = errorLogs.Where(l =>
                        string.Equals(l.Operation, "ADDPOS", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Performance logs
                var perfLogs = plist
                    .Where(l => string.Equals(l.Operation, "BG/PERF", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                double? avgFetchMs = null;
                double? avgNormalizeMs = null;
                double? avgAddToPosMs = null;
                double? avgTotalMs = null;

                if (perfLogs.Count > 0)
                {
                    double sumFetch = 0;
                    double sumNormalize = 0;
                    double sumAdd = 0;
                    double sumTotal = 0;
                    int perfCount = 0;

                    foreach (var perf in perfLogs)
                    {
                        if (string.IsNullOrWhiteSpace(perf.Message))
                        {
                            continue;
                        }

                        double f = 0, n = 0, a = 0, t = 0;
                        bool parsed = TryParsePerf(perf.Message, out f, out n, out a, out t);
                        if (!parsed)
                        {
                            continue;
                        }

                        sumFetch += f;
                        sumNormalize += n;
                        sumAdd += a;
                        sumTotal += t;
                        perfCount++;
                    }

                    if (perfCount > 0)
                    {
                        avgFetchMs = sumFetch / perfCount;
                        avgNormalizeMs = sumNormalize / perfCount;
                        avgAddToPosMs = sumAdd / perfCount;
                        avgTotalMs = sumTotal / perfCount;
                    }
                }

                DateTime? lastBgActivity = null;
                if (plist.Count > 0)
                {
                    lastBgActivity = plist.Max(l => l.TimestampUtc);
                }

                DateTime? lastErrorAt = null;
                string lastErrorMessage = null;
                if (errorLogs.Count > 0)
                {
                    var lastErr = errorLogs.OrderBy(l => l.TimestampUtc).Last();
                    lastErrorAt = lastErr.TimestampUtc;
                    lastErrorMessage = string.IsNullOrWhiteSpace(lastErr.Exception)
                        ? lastErr.Message
                        : lastErr.Exception;
                }

                DateTime? lastFetchAt = GetLastByOperation(infoLogs, "BG/FETCH");
                DateTime? lastNormalizeAt = GetLastByOperation(infoLogs, "BG/NORMALIZE");
                DateTime? lastAddToPosAt = GetLastByOperation(infoLogs, "BG/ADDPOS");

                // Health indicators
                double? minutesSinceBg = null;
                if (lastBgActivity.HasValue)
                {
                    minutesSinceBg = (now - lastBgActivity.Value).TotalMinutes;
                }

                double? minutesSinceFetch = null;
                if (lastFetchAt.HasValue)
                {
                    minutesSinceFetch = (now - lastFetchAt.Value).TotalMinutes;
                }

                // Consecutive BG errors
                int consecutiveFailures = 0;
                foreach (var l in plist.OrderByDescending(x => x.TimestampUtc))
                {
                    if (string.Equals(l.Level, "ERROR", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(l.Operation, "BG", StringComparison.OrdinalIgnoreCase))
                    {
                        consecutiveFailures++;
                        continue;
                    }

                    if (string.Equals(l.Operation, "BG/FETCH", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(l.Operation, "BG/NORMALIZE", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(l.Operation, "BG/ADDPOS", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }

                var totalCycles = bgFetch.Count + bgErrors.Count;
                double failureRate = 0;
                if (totalCycles > 0)
                {
                    failureRate = (double)bgErrors.Count / totalCycles * 100.0;
                }

                string health;
                if (bgErrors.Count > 0 || consecutiveFailures > 0)
                {
                    health = "ERROR";
                }
                else if (!lastBgActivity.HasValue || (minutesSinceBg.HasValue && minutesSinceBg > lookbackDays * 24 * 60 * 0.5))
                {
                    health = "WARNING";
                }
                else
                {
                    health = "OK";
                }

                var dto = new ProfileSyncStatsDto
                {
                    ProfileId = profile.ProfileId,
                    Name = profile.Name,
                    ProviderType = profile.ProviderType,
                    IsActive = profile.IsActive,
                    LastSyncUtc = profile.LastSyncUtc,

                    LastBackgroundActivityUtc = lastBgActivity,

                    InfoCount = infoLogs.Count,
                    ErrorCount = errorLogs.Count,

                    BgFetchCount = bgFetch.Count,
                    BgNormalizeCount = bgNormalize.Count,
                    BgAddToPosCount = bgAddToPos.Count,
                    BgErrorCount = bgErrors.Count,

                    NormalizeErrorCount = normalizeErrors.Count,
                    AddToPosErrorCount = addToPosErrors.Count,

                    HealthStatus = health,
                    ConsecutiveBgFailures = consecutiveFailures,
                    MinutesSinceLastBgActivity = minutesSinceBg,
                    MinutesSinceLastFetch = minutesSinceFetch,
                    FailureRatePercent = failureRate,

                    AvgFetchMs = avgFetchMs,
                    AvgNormalizeMs = avgNormalizeMs,
                    AvgAddToPosMs = avgAddToPosMs,
                    AvgTotalMs = avgTotalMs,

                    LastErrorAtUtc = lastErrorAt,
                    LastErrorMessage = lastErrorMessage,

                    LastFetchAtUtc = lastFetchAt,
                    LastNormalizeAtUtc = lastNormalizeAt,
                    LastAddToPosAtUtc = lastAddToPosAt
                };

                result.Add(dto);
            }

            return result;
        }


        private static bool TryParsePerf(string message, out double fetchMs, out double normalizeMs, out double addToPosMs, out double totalMs)
        {
            fetchMs = 0;
            normalizeMs = 0;
            addToPosMs = 0;
            totalMs = 0;

            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            // Expected format: BG perf: fetchMs=123; normalizeMs=456; addToPosMs=789; totalMs=1000
            var idx = message.IndexOf("fetchMs=", StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                return false;
            }

            var payload = message.Substring(idx);
            var parts = payload.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var kv = part.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (kv.Length != 2)
                {
                    continue;
                }

                var key = kv[0].Trim();
                var value = kv[1].Trim();

                if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var ms))
                {
                    continue;
                }

                if (key.IndexOf("fetchMs", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    fetchMs = ms;
                }
                else if (key.IndexOf("normalizeMs", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    normalizeMs = ms;
                }
                else if (key.IndexOf("addToPosMs", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    addToPosMs = ms;
                }
                else if (key.IndexOf("totalMs", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    totalMs = ms;
                }
            }

            return true;
        }

        private static DateTime? GetLastByOperation(IList<LogEntry> logs, string operation)
        {
            var filtered = logs.Where(l =>
                    string.Equals(l.Operation, operation, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (filtered.Count == 0)
            {
                return null;
            }

            return filtered.Max(l => l.TimestampUtc);
        }
    }
}
