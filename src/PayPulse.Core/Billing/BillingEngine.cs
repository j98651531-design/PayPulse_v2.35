using System;

namespace PayPulse.Core.Billing
{
    public class BillingEngine : IBillingEngine
    {
        private readonly IBillingRepository _repository;
        private readonly BillingTariffs _tariffs;

        public BillingEngine(IBillingRepository repository, BillingTariffs tariffs)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            if (tariffs == null) throw new ArgumentNullException("tariffs");

            _repository = repository;
            _tariffs = tariffs;
        }

        public void RecordTransfer(string profileId, string provider, string transferId)
        {
            InsertEvent("Transfer", profileId, provider, transferId);
        }

        public void RecordAddToPos(string profileId, string provider, string transferId)
        {
            InsertEvent("AddToPos", profileId, provider, transferId);
        }

        public void RecordCustomer(string profileId, string provider, string transferId)
        {
            InsertEvent("Customer", profileId, provider, transferId);
        }

        public BillingPeriodSummary GetCurrentPeriodSummary()
        {
            var now = DateTime.UtcNow;
            var periodKey = GetPeriodKey(now);
            return _repository.GetPeriodSummary(periodKey, _tariffs);
        }

        public BillingPeriod CloseCurrentPeriod()
        {
            var now = DateTime.UtcNow;
            var periodKey = GetPeriodKey(now);

            if (_repository.PeriodExists(periodKey))
            {
                var existingSummary = _repository.GetPeriodSummary(periodKey, _tariffs);

                return new BillingPeriod
                {
                    PeriodKey = periodKey,
                    FromDateUtc = GetPeriodStart(now),
                    ToDateUtc = GetPeriodEnd(now),
                    TotalTransfers = existingSummary.TransferCount,
                    TotalAddToPos = existingSummary.AddToPosCount,
                    TotalCustomers = existingSummary.CustomerCount,
                    Amount = existingSummary.TotalAmount,
                    Currency = existingSummary.Currency,
                    IsClosed = true,
                    CreatedAtUtc = now
                };
            }

            var summary = _repository.GetPeriodSummary(periodKey, _tariffs);

            var period = new BillingPeriod
            {
                PeriodKey = periodKey,
                FromDateUtc = GetPeriodStart(now),
                ToDateUtc = GetPeriodEnd(now),
                TotalTransfers = summary.TransferCount,
                TotalAddToPos = summary.AddToPosCount,
                TotalCustomers = summary.CustomerCount,
                Amount = summary.TotalAmount,
                Currency = summary.Currency,
                IsClosed = true,
                CreatedAtUtc = now
            };

            return _repository.InsertClosedPeriod(period);
        }

        private void InsertEvent(string eventType, string profileId, string provider, string transferId)
        {
            var now = DateTime.UtcNow;

            var evt = new BillingEvent
            {
                EventType = eventType,
                ProfileId = profileId,
                Provider = provider ?? string.Empty,
                TransferId = transferId ?? string.Empty,
                CreatedAtUtc = now
            };

            _repository.InsertBillingEvent(evt);
        }

        private static string GetPeriodKey(DateTime utcNow)
        {
            return utcNow.ToString("yyyy-MM");
        }

        private static DateTime GetPeriodStart(DateTime utcNow)
        {
            return new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        private static DateTime GetPeriodEnd(DateTime utcNow)
        {
            var firstDayNextMonth = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
            return firstDayNextMonth.AddMilliseconds(-1);
        }
    }
}