using System;
using System.Collections.Generic;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Providers;
using PayPulse.Domain.ValueObjects;

namespace PayPulse.Infrastructure.Providers
{
    /// <summary>
    /// WIC provider client – placeholder implementation.
    /// Each provider has different payloads and responses; this class is kept as a stub
    /// so the rest of the system can be wired without compile errors.
    /// </summary>
    public class WicProviderClient : IProviderClient, ICustomerProviderClient, IAuthProviderClient
    {
        public string ProviderType => "WIC";

        public IList<Transfer> FetchTransfers(string baseUrl, string jwtToken, string agentId, DateRange range)
        {
            // TODO: Implement real WIC fetch logic (payload, paging, mapping).
            throw new NotImplementedException("WIC FetchTransfers is not implemented yet.");
        }

        public PosCustomer FetchCustomerFromProvider(string baseUrl, string jwtToken, string idNumber, string phoneNumber)
        {
            // TODO: Implement real WIC customer lookup logic.
            throw new NotImplementedException("WIC FetchCustomerFromProvider is not implemented yet.");
        }

        public string LoginWithEmail(string baseUrl, string email, string password, string otp)
        {
            // TODO: Implement real WIC email login flow.
            throw new NotImplementedException("WIC LoginWithEmail is not implemented yet.");
        }

        public string LoginWithPhone(string baseUrl, string phoneNumber, string password, string otp)
        {
            // TODO: Implement real WIC phone login flow.
            throw new NotImplementedException("WIC LoginWithPhone is not implemented yet.");
        }
    }
}
