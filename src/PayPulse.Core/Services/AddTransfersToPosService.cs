using System;
using System.Collections.Generic;
using PayPulse.Core.Billing;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Repositories;
using PayPulse.Domain.Settings;

namespace PayPulse.Core.Services
{
    public class AddTransfersToPosService
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly ITransfersRepository _transfersRepository;
        private readonly IPosCustomerRepository _posCustomerRepository;
        private readonly IPosOperationRepository _posOperationRepository;
        private readonly LoggerService _logger;
        private readonly IBillingEngine _billingEngine;

        public AddTransfersToPosService(
            ISettingsRepository settingsRepository,
            ITransfersRepository transfersRepository,
            IPosCustomerRepository posCustomerRepository,
            IPosOperationRepository posOperationRepository,
            LoggerService logger,
            IBillingEngine billingEngine)
        {
            _settingsRepository = settingsRepository;
            _transfersRepository = transfersRepository;
            _posCustomerRepository = posCustomerRepository;
            _posOperationRepository = posOperationRepository;
            _logger = logger;
            _billingEngine = billingEngine;
        }

        public void AddPendingToPos(Profile profile, string userIdOverride, string cashboxIdOverride, string correlationId)
        {
            var settings = _settingsRepository.Load() ?? new AppSettings();
            var mappings = settings.PosMappings;

            var effectiveUserId = userIdOverride;
            var effectiveCashboxId = cashboxIdOverride;

            if (string.IsNullOrWhiteSpace(effectiveUserId) ||
                string.IsNullOrWhiteSpace(effectiveCashboxId) ||
                string.IsNullOrWhiteSpace(mappings.CurrencyUsdId) ||
                string.IsNullOrWhiteSpace(mappings.CurrencyEurId) ||
                string.IsNullOrWhiteSpace(mappings.CurrencyIlsId))
            {
                var msg =
                    "POS mappings are incomplete.\n\n" +
                    "Please open Settings and make sure you selected:\n" +
                    " - POS User / Cashbox (from the current Profile),\n" +
                    " - Currency mappings for USD, EUR and ILS.\n\n" +
                    "After saving settings, try Add to POS again.";

                _logger.Warn(msg, correlationId, null, "ADDPOS");
                throw new InvalidOperationException(msg);
            }

            IList<Transfer> pending = _transfersRepository.GetPendingForPos();
            int total = pending.Count;
            int success = 0;

            _logger.Info("AddToPos starting: pending=" + total, correlationId, null, "ADDPOS");

            foreach (var t in pending)
            {
                try
                {
                    if (string.IsNullOrEmpty(t.PosCustomerId))
                    {
                        _logger.Warn("Transfer " + t.TransactionId + " has no PosCustomerId, skipping",
                            correlationId, null, "ADDPOS");
                        continue;
                    }

                    PosCustomer customer = _posCustomerRepository.GetById(t.PosCustomerId);
                    if (customer == null)
                    {
                        _logger.Warn("Transfer " + t.TransactionId + " POS customer " + t.PosCustomerId + " not found, skipping",
                            correlationId, null, "ADDPOS");
                        continue;
                    }

                    string currencyId = mappings.CurrencyUsdId;
                    if (t.Currency == "ILS") currencyId = mappings.CurrencyIlsId;
                    else if (t.Currency == "EUR") currencyId = mappings.CurrencyEurId;

                    _posOperationRepository.InsertOperation(
                        t,
                        customer,
                        currencyId,
                        effectiveUserId,
                        effectiveCashboxId);

                    _transfersRepository.MarkAsAddedToPos(t.TransactionId);
                    if (_billingEngine != null && profile != null)
                    {
                        _billingEngine.RecordAddToPos(
                            profile.ProfileId,
                            profile.ProviderType,
                            t.TransactionId);
                    }

                    success++;
                }
                catch (Exception ex)
                {
                    _logger.Error("Error adding transfer " + t.TransactionId + " to POS",
                        ex,
                        correlationId,
                        null,
                        "ADDPOS");
                }
            }

            _logger.Info(
                "AddToPos finished: total=" + total + ", success=" + success + ", missingOrError=" + (total - success),
                correlationId,
                null,
                "ADDPOS");
        }
    }
}
