using System;
using System.Text;

namespace BitClouded.TradeBot.App.Helpers
{
    public class RequestHelper
    {
        public static string BuildOrderString(string currency, string instrument, int limit, long? since = null)
        {
            var sb = new StringBuilder();
            // These need to be in this specific order for the API to work
            sb.Append("{\"currency\":\"");
            sb.Append(currency);
            sb.Append("\",\"instrument\":\"");
            sb.Append(instrument);
            sb.Append("\",\"limit\":");
            sb.Append(limit);
            if (since != null)
            {
                sb.Append(",\"since\":");
                sb.Append(since.Value);
            }

            sb.Append("}");
            return sb.ToString();
        }

        public static string BuildNewOrderString(string currency, string instrument, long price, int volume,
            string orderSide, string orderType)
        {
            var sb = new StringBuilder();
            // These need to be in this specific order for the API to work
            sb.Append("{\"currency\":\"");
            sb.Append(currency);
            sb.Append("\",\"instrument\":\"");
            sb.Append(instrument);
            sb.Append("\",\"price\":");
            sb.Append(price);
            sb.Append(",\"volume\":");
            sb.Append(volume);
            sb.Append(",\"orderSide\":\"");
            sb.Append(orderSide);
            sb.Append("\",\"ordertype\":\"");
            sb.Append(orderType);
            sb.Append("\",\"clientRequestId\":\"");
            sb.Append(Guid.NewGuid());
            sb.Append("\"}");
            return sb.ToString();
        }

        public static string BuildDefaultOrderString()
        {
            return BuildOrderString("AUD", "BTC", 10, 1);
        }

    }


}