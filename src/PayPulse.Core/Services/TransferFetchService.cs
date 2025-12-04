using System;
using System.Collections.Generic;
using PayPulse.Core.DTOs;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Providers;
using PayPulse.Domain.Interfaces.Repositories;
using PayPulse.Domain.Interfaces.Services;
using PayPulse.Domain.Settings;
using PayPulse.Domain.ValueObjects;
using PayPulse.Core.Billing;

namespace PayPulse.Core.Services
{
    public class TransferFetchService
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly ITransfersRepository _transfersRepository;
        private readonly IProviderClient _providerClient;
        private readonly ITokenDecoder _tokenDecoder;
        private readonly IDateTimeProvider _clock;
        private readonly LoggerService _logger;
        private readonly IBillingEngine _billingEngine;

        public TransferFetchService(
            ISettingsRepository settingsRepository,
            ITransfersRepository transfersRepository,
            IProviderClient providerClient,
            ITokenDecoder tokenDecoder,
            IDateTimeProvider clock,
            LoggerService logger,
            IBillingEngine billingEngine)
        {
            _settingsRepository = settingsRepository;
            _transfersRepository = transfersRepository;
            _providerClient = providerClient;
            _tokenDecoder = tokenDecoder;
            _clock = clock;
            _logger = logger;
            _billingEngine = billingEngine;
        }

        public IList<TransferViewDto> FetchForProfile(
            DateRangeRequestDto request,
            Profile profile,
            string jwtToken,
            string correlationId)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            var settings = _settingsRepository.Load() ?? new AppSettings();

            ProviderSettings providerSettings = null;
            if (settings.Providers != null)
            {
                providerSettings = settings.Providers.Get(profile.ProviderType);
            }

            var baseUrl = providerSettings != null && !string.IsNullOrWhiteSpace(providerSettings.BaseUrl)
                ? providerSettings.BaseUrl
                : settings.StbApi.BaseUrl;

            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                throw new InvalidOperationException("No JWT token available for fetch.");
            }

            var tokenInfo = _tokenDecoder.Decode(jwtToken);
            var agentId = tokenInfo == null ? null : tokenInfo.AgentId;

            var range = new DateRange { Start = request.Start, End = request.End };
            _logger.Info("Fetching transfers " + range.Start + " - " + range.End,
                correlationId,
                profile.ProfileId,
                "FETCH");

            var transfers = _providerClient.FetchTransfers(baseUrl, jwtToken, agentId, range);
            foreach (var t in transfers)
            {
                if (t.CreatedAtUtc == default(DateTime))
                {
                    t.CreatedAtUtc = _clock.UtcNow;
                }

                var isNew = !_transfersRepository.TransferExists(t.TransactionId);
                _transfersRepository.UpsertTransfer(t);

                if (isNew && _billingEngine != null)
                {
                    _billingEngine.RecordTransfer(
                        profile.ProfileId,
                        profile.ProviderType,
                        t.TransactionId);
                }
            }

            var fromDb = _transfersRepository.GetTransfers(range, agentId);
            var result = new List<TransferViewDto>();
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
