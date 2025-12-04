using System;
using System.Collections.Generic;

namespace PayPulse.Core.Billing
{
    public interface IBillingRepository
    {
        void InsertBillingEvent(BillingEvent billingEvent);
        BillingTariffs GetTariffs();
        void SaveTariffs(BillingTariffs tariffs);
        BillingPeriodSummary GetPeriodSummary(string periodKey, BillingTariffs tariffs);
        BillingPeriod InsertClosedPeriod(BillingPeriod period);
        bool PeriodExists(string periodKey);
        IList<BillingEvent> GetEvents(DateTime fromUtc, DateTime toUtc, string eventType, string profileId, string provider);
        IList<BillingPeriod> GetClosedPeriods();
    }

    public interface IBillingEngine
    {
        void RecordTransfer(string profileId, string provider, string transferId);
        void RecordAddToPos(string profileId, string provider, string transferId);
        void RecordCustomer(string profileId, string provider, string transferId);
        BillingPeriodSummary GetCurrentPeriodSummary();
        BillingPeriod CloseCurrentPeriod();
    }
}