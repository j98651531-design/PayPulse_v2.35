using System.Collections.Generic;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Repositories;

namespace PayPulse.Core.Services
{
    public class PosMetadataService
    {
        private readonly IPosMetadataRepository _repo;

        public PosMetadataService(IPosMetadataRepository repo)
        {
            _repo = repo;
        }

        public IList<CurrencyRef> GetCurrencies()
        {
            return _repo.GetCurrencies();
        }

        public IList<UserRef> GetUsers()
        {
            return _repo.GetUsers();
        }

        public IList<CashboxRef> GetCashboxes()
        {
            return _repo.GetCashboxes();
        }

        public IList<IdTypeRef> GetIdTypes()
        {
            return _repo.GetIdTypes();
        }

        public IList<UserTypeRef> GetUserTypes()
        {
            return _repo.GetUserTypes();
        }
    }
}
