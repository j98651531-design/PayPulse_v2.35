using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using PayPulse.Domain.Entities;
using PayPulse.Domain.Interfaces.Providers;
using PayPulse.Domain.ValueObjects;

namespace PayPulse.Infrastructure.Providers
{
    public class StbProviderClient : IProviderClient, ICustomerProviderClient, IAuthProviderClient
    {
        public string ProviderType { get { return "STB"; } }

        private readonly HttpClient _http = new HttpClient();

        public IList<Transfer> FetchTransfers(string baseUrl, string jwtToken, string agentId, DateRange range)
        {
            string url = baseUrl.TrimEnd('/') + "/report/transactionReport";

            var payload = new
            {
                agents = new[] { agentId },
                fullResult = false,
                startDate = range.Start.ToString("yyyy-MM-dd HH:mm:00"),
                endDate = range.End.ToString("yyyy-MM-dd HH:mm:00"),
                sortField = "DATE",
                sortDirection = "DESC",
                pageNum = 0
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
            request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = _http.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            string json = response.Content.ReadAsStringAsync().Result;

            dynamic obj = JsonConvert.DeserializeObject(json);
            var list = new List<Transfer>();

            foreach (var row in obj.reportData)
            {
                var t = new Transfer();
                t.TransactionId = (string)row.transactionId;
                t.ReservationCode = (string)row.reservationCode;
                t.Date = DateTime.Parse((string)row.date).ToLocalTime();
                t.SenderName = (string)row.sender;
                t.TellerName = (string)row.tellerName;
                t.AgentName = null;
                t.SentAmount = decimal.Parse((string)row.amountSent);
                t.FeeAmount = decimal.Parse((string)row.transactionFee);
                t.ExtraAmount = 0m;
                t.Currency = (string)row.sendCurrency;
                t.PosAmount = t.SentAmount + t.FeeAmount + t.ExtraAmount;
                t.AgentId = agentId;
                t.SenderIdType = (string)row.userIdType;
                t.SenderIdNumber = (string)row.userIdNumber;
                t.SenderPhoneNumber = (string)row.senderPhoneNumber;
                t.Type = (string)row.type;
                t.Status = (string)row.status;
                list.Add(t);
            }

            return list;
        }

        public PosCustomer FetchCustomerFromProvider(string baseUrl, string jwtToken, string idNumber, string phoneNumber)
        {
            string url = baseUrl.TrimEnd('/') + "/user/queryOrg";
            string query = !string.IsNullOrWhiteSpace(phoneNumber) ? phoneNumber : idNumber;
            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }

            var payload = new
            {
                customerActionRequiredReasons = true,
                query = query,
                queryType = string.IsNullOrWhiteSpace(phoneNumber) ? "ID" : "PHONE",
                origin = "DASHBOARD"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
            request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = _http.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            string json = response.Content.ReadAsStringAsync().Result;

            dynamic obj = JsonConvert.DeserializeObject(json);
            if (obj.status != "SUCCESS")
            {
                return null;
            }

            dynamic r = obj.result;

            var c = new PosCustomer();
            c.FirstName = (string)r.firstName;
            c.LastName = (string)r.lastName;
            c.PhoneNumber = (string)r.phoneNumber;
            c.IdNumber = (string)r.idNumber;
            return c;
        }

        public string LoginWithEmail(string baseUrl, string email, string password, string otp)
        {
            var payload = new
            {
                email = email,
                password = password,
                origin = "DASHBOARD",
                timeSend = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                verificationCode = otp
            };
            return PostLogin(baseUrl, payload);
        }

        public string LoginWithPhone(string baseUrl, string phoneNumber, string password, string otp)
        {
            var payload = new
            {
                phoneNumber = phoneNumber,
                password = password,
                origin = "DASHBOARD",
                timeSend = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                verificationCode = otp
            };
            return PostLogin(baseUrl, payload);
        }

        private string PostLogin(string baseUrl, object payload)
        {
            string url = baseUrl.TrimEnd('/') + "/unAuth/login";
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = _http.PostAsync(url, content).Result;
            response.EnsureSuccessStatusCode();
            string json = response.Content.ReadAsStringAsync().Result;

            dynamic obj = JsonConvert.DeserializeObject(json);
            int statusCode = (int)(obj.statusCode ?? 0);
            if (statusCode != 0)
            {
                string message = obj.message != null ? (string)obj.message : "Provider error";
                throw new InvalidOperationException("Auth failed. StatusCode=" + statusCode + ", Message=" + message);
            }

            string token = obj.token;
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("Auth response did not contain a token.");
            }

            return token;
        }
    }
}
