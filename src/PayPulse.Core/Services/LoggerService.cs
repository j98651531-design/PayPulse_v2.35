using System;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Logging;
using PayPulse.Domain.Interfaces.Repositories;
using PayPulse.Domain.Interfaces.Services;

namespace PayPulse.Core.Services
{
    public class LoggerService
    {
        private readonly ILogRepository _repo;
        private readonly IDateTimeProvider _clock;
        private readonly CurrentUserContext _currentUserContext;

        public LoggerService(ILogRepository repo, IDateTimeProvider clock, CurrentUserContext currentUserContext)
        {
            _repo = repo;
            _clock = clock;
            _currentUserContext = currentUserContext;
        }

        public void Info(string message, string correlationId, string profileId, string operation)
        {
            Log("INFO", message, null, correlationId, profileId, operation);
        }

        public void Warn(string message, string correlationId, string profileId, string operation)
        {
            Log("WARN", message, null, correlationId, profileId, operation);
        }

        public void Error(string message, Exception ex, string correlationId, string profileId, string operation)
        {
            Log("ERROR", message, ex, correlationId, profileId, operation);
        }

        private void Log(string level, string message, Exception ex, string correlationId, string profileId, string operation)
        {
            var log = new LogEntry
            {
                LogId = Guid.NewGuid().ToString(),
                TimestampUtc = _clock.UtcNow,
                Level = level,
                Message = message,
                ProfileId = profileId,
                Operation = operation,
                Exception = ex == null ? null : ex.ToString(),
                CorrelationId = correlationId,
                UserId = _currentUserContext.CurrentUser?.UserId,
                UserName = _currentUserContext.CurrentUser?.UserName
            };
            _repo.Insert(log);
            LogEventHub.Raise(log);
        }
    }
}
