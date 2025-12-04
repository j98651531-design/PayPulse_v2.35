using System;

namespace PayPulse.Domain.Entities
{
    public class LogEntry
    {
        public string LogId { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string Level { get; set; } // INFO/WARN/ERROR
        public string Message { get; set; }
        public string ProfileId { get; set; }
        public string Operation { get; set; }
        public string Exception { get; set; }
        public string CorrelationId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
