using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BitClouded.TradeBot.DataAccess
{
    public class MarketClient
    {
        private readonly IBtcRequestClient _requestClient;

        public MarketClient(IBtcRequestClient requestClient)
        {
            _requestClient = requestClient;
        }

        public Trade[] GetTrades(
            string currency, string instrument)
        {
            var path = $"/market/{instrument}/{currency}/trades";//?since=591540122";

            var result = _requestClient.SendRequest(path);
            Console.WriteLine(result);
            return JsonConvert.DeserializeObject<Trade[]>(result);
        }

        public OrderBookResponse GetOrderBook(
            string currency, string instrument)
        {
            var path = $"/market/{instrument}/{currency}/orderbook";

            var result = _requestClient.SendRequest(path);

            return JsonConvert.DeserializeObject<OrderBookResponse>(result);
        }
    }

    public class Trade
    {
        public int Tid { get; set; }
        public double Amount { get; set; }

        public decimal Price { get; set; }

        public long Date { get; set; }
    }

    public class OrderBookResponse
    {
        public string Currency { get; set; }

        public string Instrument { get; set; }

        public long Timestamp { get; set; }

        public double[][] Asks { get; set; }

        public double[][] Bids { get; set; }
    }
}
