using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BitClouded.TradeBot.DataAccess
{
    public class OrdersClient
    {
        private readonly IBtcRequestClient _requestClient;

        public OrdersClient(IBtcRequestClient requestClient)
        {
            _requestClient = requestClient;
        }

        public OrderDetails GetOrder(int[] orderIds)
        {
            var sb = new StringBuilder();
            sb.Append("{\"orderIds\":");
            sb.Append("[");
            sb.Append(string.Join(",", orderIds));
            sb.Append("]");
            sb.Append("}");
            var response = _requestClient.SendRequest("/order/detail", sb.ToString());
            return JsonConvert.DeserializeObject<OrderDetails>(response);
        }

        public OrderCancellations CancelOrder(int[] orderIds)
        {
            Console.WriteLine($"Cancelling: {string.Join(",", orderIds)}");
            var sb = new StringBuilder();
            sb.Append("{\"orderIds\":");
            sb.Append("[");
            sb.Append(string.Join(",", orderIds));
            sb.Append("]");
            sb.Append("}");
            var response = _requestClient.SendRequest("/order/cancel", sb.ToString());
            return JsonConvert.DeserializeObject<OrderCancellations>(response);
        }

        public CreateOrderResponse CreateOrder(
            string currency, string instrument, long price, long volume, string orderSide)
        {
            Console.WriteLine($"Placing {orderSide} @ {price} x {volume}");
            var clientRequestId = Guid.NewGuid().ToString();
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
            sb.Append("Limit");
            sb.Append("\",\"clientRequestId\":\"");
            sb.Append(clientRequestId);
            sb.Append("\"}");
            var response = _requestClient.SendRequest("/order/create", sb.ToString());
            return JsonConvert.DeserializeObject<CreateOrderResponse>(response);
        }
    }

    public class CreateOrderResponse
    {
        public bool Success { get; set; }

        public int? ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public int Id { get; set; }

        public string ClientRequestId { get; set; }
    }

    public class BaseMessage<T>
    {
        public bool Success { get; set; }

        public int? ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public T[] Orders { get; set; }
    }

    public class OrderCancellations : BaseMessage<OrderCancellation>
    {
        
    }

    public class OrderCancellation
    {
        public bool Success { get; set; }

        public int? ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public int Id { get; set; }
    }

    public class OrderDetails
    {
        public bool Success { get; set; }

        public int? ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public OrderDetail[] Orders { get; set; }
    }

    public class OrderDetail
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public string OrderSide { get; set; }

        public TradeDetail[] Trades { get; set; }

        public long OpenVolume { get; set; }
    }

    public class TradeDetail
    {
        public int Id { get; set; }

        public long Price { get; set; }

        public long Volume { get; set; }

        public long Fee { get; set; }
    }
}
