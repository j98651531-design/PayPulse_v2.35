using System;

namespace PayPulse.Core.Billing
{
    public class BillingEvent
    {
        public int Id { get; set; }
        public string EventType { get; set; }      // "Transfer" | "AddToPos" | "Customer"
        public string ProfileId { get; set; }
        public string Provider { get; set; }
        public string TransferId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class BillingPeriodSummary
    {
        public string PeriodKey { get; set; }      // "YYYY-MM"
        public int TransferCount { get; set; }
        public int AddToPosCount { get; set; }
        public int CustomerCount { get; set; }

        public decimal PricePerTransfer { get; set; }
        public decimal PricePerAddToPos { get; set; }
        public decimal PricePerCustomer { get; set; }

        public string Currency { get; set; }

        public decimal TotalAmount
        {
            get
            {
                return (TransferCount * PricePerTransfer)
                     + (AddToPosCount * PricePerAddToPos)
                     + (CustomerCount * PricePerCustomer);
            }
        }
    }

    public class BillingPeriod
    {
        public int Id { get; set; }
        public string PeriodKey { get; set; }
        public DateTime FromDateUtc { get; set; }
        public DateTime ToDateUtc { get; set; }
        public int TotalTransfers { get; set; }
        public int TotalAddToPos { get; set; }
        public int TotalCustomers { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public bool IsClosed { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class BillingTariffs
    {
        public decimal PricePerTransfer { get; set; }
        public decimal PricePerAddToPos { get; set; }
        public decimal PricePerCustomer { get; set; }
        public string Currency { get; set; }
    }
}