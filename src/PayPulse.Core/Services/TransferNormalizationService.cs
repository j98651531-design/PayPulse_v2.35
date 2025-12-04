using System;
using PayPulse.Core.DTOs;
using PayPulse.Core.Billing;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Providers;
using PayPulse.Domain.Interfaces.Repositories;
using PayPulse.Domain.Interfaces.Services;
using PayPulse.Domain.Settings;
using PayPulse.Domain.ValueObjects;

namespace PayPulse.Core.Services
{
    public class TransferNormalizationService
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly ITransfersRepository _transfersRepository;
        private readonly IPosCustomerRepository _posCustomerRepository;
        private readonly ICustomerProviderClient _customerProviderClient;
        private readonly ITokenDecoder _tokenDecoder;
        private readonly LoggerService _logger;
        private readonly IBillingEngine _billingEngine;

        public TransferNormalizationService(
            ISettingsRepository settingsRepository,
            ITransfersRepository transfersRepository,
            IPosCustomerRepository posCustomerRepository,
            ICustomerProviderClient customerProviderClient,
            ITokenDecoder tokenDecoder,
            LoggerService logger,
            IBillingEngine billingEngine)
        {
            _settingsRepository = settingsRepository;
            _transfersRepository = transfersRepository;
            _posCustomerRepository = posCustomerRepository;
            _customerProviderClient = customerProviderClient;
            _tokenDecoder = tokenDecoder;
            _logger = logger;
            _billingEngine = billingEngine;
        }

        public void NormalizeForProfile(
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
                throw new InvalidOperationException("No JWT token for normalization.");
            }

            var tokenInfo = _tokenDecoder.Decode(jwtToken);
            var agentId = tokenInfo == null ? null : tokenInfo.AgentId;

            var range = new DateRange { Start = request.Start, End = request.End };
            var transfers = _transfersRepository.GetTransfers(range, agentId);

            foreach (var t in transfers)
            {
                try
                {
                    if (!string.IsNullOrEmpty(t.PosCustomerId))
                    {
                        continue;
                    }

                    PosCustomer existing = null;
                    if (!string.IsNullOrWhiteSpace(t.SenderIdNumber))
                    {
                        existing = _posCustomerRepository.FindByIdNumber(t.SenderIdNumber);
                    }

                    if (existing == null && !string.IsNullOrWhiteSpace(t.SenderPhoneNumber))
                    {
                        existing = _posCustomerRepository.FindByPhone(t.SenderPhoneNumber);
                    }

                    if (existing != null)
                    {
                        _transfersRepository.UpdateTransferCustomer(
                            t.TransactionId,
                            existing.CustomerId,
                            false,
                            null);
                        continue;
                    }

                    var fromProvider = _customerProviderClient.FetchCustomerFromProvider(
                        baseUrl,
                        jwtToken,
                        t.SenderIdNumber,
                        t.SenderPhoneNumber);

                    if (fromProvider == null)
                    {
                        _transfersRepository.UpdateTransferCustomer(
                            t.TransactionId,
                            null,
                            false,
                            "Customer not found");
                        continue;
                    }

                    var newId = _posCustomerRepository.Insert(fromProvider);
                    _transfersRepository.UpdateTransferCustomer(
                        t.TransactionId,
                        newId,
                        true,
                        null);

                    if (_billingEngine != null && profile != null)
                    {
                        _billingEngine.RecordCustomer(
                            profile.ProfileId,
                            profile.ProviderType,
                            t.TransactionId);
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error("Normalize failed for " + t.TransactionId, ex, correlationId, null, "NORMALIZE");
                    _transfersRepository.UpdateTransferCustomer(
                        t.TransactionId,
                        t.PosCustomerId,
                        t.IsNewCustomer,
                        "Normalize error: " + ex.Message);
                }
            }
        }
    }
}
