using System;
using PayPulse.Domain.ValueObjects;

namespace PayPulse.Domain.Interfaces.Services
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
        DateTime Today { get; }
    }

    public interface ITokenDecoder
    {
        TokenInfo Decode(string jwtToken);
    }
}
