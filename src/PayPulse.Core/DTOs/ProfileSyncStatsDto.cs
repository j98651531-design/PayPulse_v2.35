using System;

namespace PayPulse.Core.DTOs
{
    public class ProfileSyncStatsDto
    {
        public string ProfileId { get; set; }
        public string Name { get; set; }
        public string ProviderType { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastSyncUtc { get; set; }

        public DateTime? LastBackgroundActivityUtc { get; set; }

        public int InfoCount { get; set; }
        public int ErrorCount { get; set; }

        // Background sync phase counters (derived from logs)
        public int BgFetchCount { get; set; }
        public int BgNormalizeCount { get; set; }
        public int BgAddToPosCount { get; set; }
        public int BgErrorCount { get; set; }

        // Detailed error buckets
        public int NormalizeErrorCount { get; set; }
        public int AddToPosErrorCount { get; set; }

        // Health indicators
        public string HealthStatus { get; set; }
        public int ConsecutiveBgFailures { get; set; }
        public double? MinutesSinceLastBgActivity { get; set; }
        public double? MinutesSinceLastFetch { get; set; }
        public double FailureRatePercent { get; set; }

        // Performance timings (averages over BG/PERF logs in the window)
        public double? AvgFetchMs { get; set; }
        public double? AvgNormalizeMs { get; set; }
        public double? AvgAddToPosMs { get; set; }
        public double? AvgTotalMs { get; set; }

        public DateTime? LastErrorAtUtc { get; set; }
        public string LastErrorMessage { get; set; }

        public DateTime? LastFetchAtUtc { get; set; }
        public DateTime? LastNormalizeAtUtc { get; set; }
        public DateTime? LastAddToPosAtUtc { get; set; }
    }
}
