using System;

namespace PayPulse.Domain.Entities
{
    public class Transfer
    {
        public string TransactionId { get; set; }
        public string ReservationCode { get; set; }
        public DateTime Date { get; set; }

        public string SenderName { get; set; }
        public string TellerName { get; set; }
        public string AgentName { get; set; }

        public decimal SentAmount { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal ExtraAmount { get; set; }
        public decimal PosAmount { get; set; }

        public string Currency { get; set; }
        public string AgentId { get; set; }
        public string SenderIdType { get; set; }
        public string SenderIdNumber { get; set; }
        public string SenderPhoneNumber { get; set; }

        public string Type { get; set; }
        public string Status { get; set; }
        public string SenderNationality { get; set; }

        public decimal PosCustomerTotalAmount { get; set; }
        public string PosCustomerId { get; set; }
        public bool IsNewCustomer { get; set; }
        public bool IsAddedToPos { get; set; }
        public string ErrorMessage { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
