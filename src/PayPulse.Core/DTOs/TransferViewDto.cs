using System;

namespace PayPulse.Core.DTOs
{
    public class TransferViewDto
    {
        public string TransactionId { get; set; }
        public string ReservationCode { get; set; }
        public DateTime Date { get; set; }
        public string SenderName { get; set; }
        public string TellerName { get; set; }
        public string AgentName { get; set; }
        public decimal PosAmount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public bool IsNewCustomer { get; set; }
        public bool IsAddedToPos { get; set; }
        public string ErrorMessage { get; set; }
    }
}
