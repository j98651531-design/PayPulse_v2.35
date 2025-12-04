using System;
using PayPulse.Domain.Entities;

namespace PayPulse.Domain.Logging
{
    /// <summary>
    /// In-memory log event hub used for real-time log monitoring (Live Logs form).
    /// LoggerService raises this event whenever a new LogEntry is written to the repository.
    /// WinForms subscribers can listen and update the UI.
    /// </summary>
    public static class LogEventHub
    {
        public static event EventHandler<LogEntry> LogAdded;

        public static void Raise(LogEntry entry)
        {
            var handler = LogAdded;
            if (handler != null)
            {
                handler(null, entry);
            }
        }
    }
}
