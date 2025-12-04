using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json.Linq;
using PayPulse.Domain.Interfaces.Services;
using PayPulse.Domain.ValueObjects;

namespace PayPulse.Infrastructure.Security
{
    public class JwtTokenDecoder : ITokenDecoder
    {
        public TokenInfo Decode(string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                return new TokenInfo();
            }

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);

            var info = new TokenInfo();

            //object accountsObj;
            //if (token.Payload.TryGetValue("accounts", out accountsObj))
            //{
            //    JArray arr = accountsObj as JArray;
            //    if (arr != null && arr.Count > 0)
            //    {
            //        JObject first = arr[0] as JObject;
            //        if (first != null)
            //        {
            //            info.AgentId = (string)first["connectedEntityId"];
            //        }
            //    }
            //}

            object accountsObj;
            if (token.Payload.TryGetValue("accounts", out accountsObj))
            {
                // 1. Convert the object to a string (which is how the complex JSON is stored).
                string accountsJson = accountsObj?.ToString();

                if (!string.IsNullOrEmpty(accountsJson))
                {
                    try
                    {
                        // 2. Use JArray.Parse() to convert the JSON string into a JArray.
                        JArray arr = JArray.Parse(accountsJson);

                        if (arr != null && arr.Count > 0)
                        {
                            JObject first = arr[0] as JObject;
                            if (first != null)
                            {
                                info.AgentId = (string)first["connectedEntityId"];
                            }
                        }
                    }
                    catch (Newtonsoft.Json.JsonReaderException)
                    {
                        // Handle cases where the claim is not valid JSON
                        // e.g., log the error or return a default value
                    }
                }
            }

            object uid;
            if (token.Payload.TryGetValue("userId", out uid))
            {
                info.UserId = uid == null ? null : uid.ToString();
            }

            return info;
        }
    }
}
