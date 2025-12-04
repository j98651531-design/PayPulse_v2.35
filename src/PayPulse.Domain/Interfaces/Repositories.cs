using System;
using System.Collections.Generic;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Settings;
using PayPulse.Domain.ValueObjects;

namespace PayPulse.Domain.Interfaces.Repositories
{
    public interface ISettingsRepository
    {
        AppSettings Load();
        void Save(AppSettings settings);
    }

    public interface ITransfersRepository
    {
        void UpsertTransfer(Transfer t);
        bool TransferExists(string transactionId);
        IList<Transfer> GetTransfers(DateRange range, string agentId = null);
        void UpdateTransferCustomer(string transactionId, string posCustomerId, bool isNewCustomer, string errorMessage);
        void MarkAsAddedToPos(string transactionId);
        IList<Transfer> GetPendingForPos();
    }

    public interface IPosCustomerRepository
    {
        PosCustomer FindByIdNumber(string idNumber);
        PosCustomer FindByPhone(string phoneNumber);
        PosCustomer GetById(string customerId);
        string Insert(PosCustomer c);
    }

    public interface IPosOperationRepository
    {
        void InsertOperation(Transfer t, PosCustomer c, string currencyId, string userId, string cashboxId);
    }

    public interface IPosMetadataRepository
    {
        IList<CurrencyRef> GetCurrencies();
        IList<UserRef> GetUsers();
        IList<CashboxRef> GetCashboxes();
        IList<IdTypeRef> GetIdTypes();
        IList<UserTypeRef> GetUserTypes();
    }

    public interface ILogRepository
    {
        void Insert(LogEntry log);
        IList<LogEntry> Get(DateTime fromUtc, DateTime toUtc);
    }

    public interface IProfileRepository
    {
        IList<Profile> GetAll();
        Profile GetById(string profileId);
        void Insert(Profile profile);
        void Update(Profile profile);
        void Delete(string profileId);
    }

    public interface IAppUserRepository
    {
        AppUser GetByUserName(string userName);
        IList<AppUser> GetAll();
        void Insert(AppUser user);
        void Update(AppUser user);
        void Delete(string userId);

        /// <summary>
        /// Updates the LastLoginAt timestamp (and UpdatedAt) for the user.
        /// </summary>
        void UpdateLastLogin(string userId, DateTime lastLoginUtc);

        /// <summary>
        /// Updates the password (plain text provided, will be hashed inside),
        /// and optionally clears the MustChangePassword flag.
        /// </summary>
        void UpdatePassword(string userId, string newPassword, bool clearMustChangePassword);
    }


}
