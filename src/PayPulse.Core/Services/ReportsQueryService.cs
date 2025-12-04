using System.Collections.Generic;
using PayPulse.Core.DTOs;
using PayPulse.Domain.Interfaces.Repositories;
using PayPulse.Domain.Interfaces.Services;
using PayPulse.Domain.ValueObjects;

namespace PayPulse.Core.Services
{
    public class ReportsQueryService
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly ITransfersRepository _transfersRepository;
        private readonly ITokenDecoder _tokenDecoder;

        public ReportsQueryService(
            ISettingsRepository settingsRepository,
            ITransfersRepository transfersRepository,
            ITokenDecoder tokenDecoder)
        {
            _settingsRepository = settingsRepository;
            _transfersRepository = transfersRepository;
            _tokenDecoder = tokenDecoder;
        }

        public IList<TransferViewDto> GetReportForToken(DateRangeRequestDto request, string jwtToken)
        {
            var result = new List<TransferViewDto>();

            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                return result;
            }

            var tokenInfo = _tokenDecoder.Decode(jwtToken);
            var agentId = tokenInfo == null ? null : tokenInfo.AgentId;

            var range = new DateRange { Start = request.Start, End = request.End };
            var fromDb = _transfersRepository.GetTransfers(range, agentId);

            foreach (var t in fromDb)
            {
                result.Add(new TransferViewDto
                {
                    TransactionId = t.TransactionId,
                    ReservationCode = t.ReservationCode,
                    Date = t.Date,
                    SenderName = t.SenderName,
                    TellerName = t.TellerName,
                    AgentName = t.AgentName,
                    PosAmount = t.PosAmount,
                    Currency = t.Currency,
                    Status = t.Status,
                    IsNewCustomer = t.IsNewCustomer,
                    IsAddedToPos = t.IsAddedToPos,
                    ErrorMessage = t.ErrorMessage
                });
            }

            return result;
        }
    }
}
