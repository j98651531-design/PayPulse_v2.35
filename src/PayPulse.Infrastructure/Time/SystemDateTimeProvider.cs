using System;
using PayPulse.Domain.Interfaces.Services;

namespace PayPulse.Infrastructure.Time
{
    public class SystemDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow { get { return DateTime.UtcNow; } }
        public DateTime Today { get { return DateTime.Today; } }
    }
}
