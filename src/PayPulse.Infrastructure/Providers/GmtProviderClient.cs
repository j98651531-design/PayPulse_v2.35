using System;
using System.Collections.Generic;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Providers;
using PayPulse.Domain.ValueObjects;

namespace PayPulse.Infrastructure.Providers
{
    /// <summary>
    /// GMT provider client – placeholder implementation.
    /// Each provider has different payloads and responses; this class is kept as a stub
    /// so the rest of the system can be wired without compile errors.
    /// </summary>
    public class GmtProviderClient : IProviderClient, ICustomerProviderClient, IAuthProviderClient
    {
        public string ProviderType => "GMT";

        public IList<Transfer> FetchTransfers(string baseUrl, string jwtToken, string agentId, DateRange range)
        {
            // TODO: Implement real GMT fetch logic (payload, paging, mapping).
            throw new NotImplementedException("GMT FetchTransfers is not implemented yet.");
        }

        public PosCustomer FetchCustomerFromProvider(string baseUrl, string jwtToken, string idNumber, string phoneNumber)
        {
            // TODO: Implement real GMT customer lookup logic.
            throw new NotImplementedException("GMT FetchCustomerFromProvider is not implemented yet.");
        }

        public string LoginWithEmail(string baseUrl, string email, string password, string otp)
        {
            // TODO: Implement real GMT email login flow.
            throw new NotImplementedException("GMT LoginWithEmail is not implemented yet.");
        }

        public string LoginWithPhone(string baseUrl, string phoneNumber, string password, string otp)
        {
            // TODO: Implement real GMT phone login flow.
            throw new NotImplementedException("GMT LoginWithPhone is not implemented yet.");
        }
    }
}
