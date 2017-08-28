using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Deserializers;

namespace BitClouded.TradeBot.DataAccess
{
    public interface IBtcRequestClient
    {
        string SendRequest(string action, string postData = null);
    }

    public class BtcRequestClient : IBtcRequestClient
    {
        private const string BaseUrl = "https://api.btcmarkets.net";
        private readonly IRestClient _client;
        private static readonly DateTime UnixTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public BtcRequestClient()
        {
            _client = new RestClient(BaseUrl);
            _client.ClearHandlers();
            _client.AddHandler("application/json", new JsonDeserializer());
        }

        public static string ComputeHash(string privateKey, string data)
        {
            var encoding = System.Text.Encoding.UTF8;
            using (var hasher = new HMACSHA512(Convert.FromBase64String(privateKey)))
            {
                return Convert.ToBase64String(hasher.ComputeHash(encoding.GetBytes(data)));
            }
        }

        public static string BuildStringToSign(string action, string postData, string timestamp)
        {
            var stringToSign = new StringBuilder();
            stringToSign.Append(action + "\n");
            stringToSign.Append(timestamp + "\n");
            if (!string.IsNullOrWhiteSpace(postData))
            {
                stringToSign.Append(postData);
            }

            return stringToSign.ToString();
        }

        public string SendRequest(string action, string postData = null)
        {
            //get the epoch timestamp to be used as the nonce
            var timestamp = ((long)(DateTime.UtcNow - UnixTime)
                .TotalMilliseconds)
                .ToString(CultureInfo.InvariantCulture);

            // create the string that needs to be signed
            var stringToSign = BuildStringToSign(action, postData, timestamp);

            // build signature to be included in the http header
            var signature = ComputeHash(ApplicationConstants.PrivateKey, stringToSign);

            var request = new RestRequest(action)
            {
                Method = postData != null ? Method.POST : Method.GET
            };
            request.AddHeader("Accept", Content);
            request.AddHeader("Accept-Charset", Encoding);
            request.AddHeader("Content-Type", Content);
            request.AddHeader(ApikeyHeader, ApplicationConstants.ApiKey);
            request.AddHeader(SignatureHeader, signature);
            request.AddHeader(TimestampHeader, timestamp);
            if (postData != null)
            {
                request.AddParameter("application/json", postData, ParameterType.RequestBody);
            }

            var queryResult = _client.Execute(request);

            return queryResult.Content;
        }

        public const string ApikeyHeader = "apikey";
        public const string TimestampHeader = "timestamp";
        public const string SignatureHeader = "signature";
        public const string Encoding = "UTF-8";
        public const string Content = "application/json";
    }
}
