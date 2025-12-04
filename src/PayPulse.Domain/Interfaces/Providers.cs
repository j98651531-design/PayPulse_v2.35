using System.Collections.Generic;
using PayPulse.Domain.Entities;
using PayPulse.Domain.ValueObjects;

namespace PayPulse.Domain.Interfaces.Providers
{
    public interface IProviderClient
    {
        string ProviderType { get; }
        IList<Transfer> FetchTransfers(string baseUrl, string jwtToken, string agentId, DateRange range);
    }

    public interface ICustomerProviderClient
    {
        PosCustomer FetchCustomerFromProvider(string baseUrl, string jwtToken, string idNumber, string phoneNumber);
    }

    public interface IAuthProviderClient
    {
        string LoginWithEmail(string baseUrl, string email, string password, string otp);
        string LoginWithPhone(string baseUrl, string phoneNumber, string password, string otp);
    }
}
