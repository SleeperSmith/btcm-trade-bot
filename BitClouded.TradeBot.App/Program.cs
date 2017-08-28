using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BitClouded.TradeBot.App.Constants;
using BitClouded.TradeBot.App.Helpers;
using BitClouded.TradeBot.DataAccess;
using RestSharp;
using RestSharp.Deserializers;

namespace BitClouded.TradeBot.App
{
    class Program
    {
        static void Main(string[] args)
        {
            //ReportPrice();
            Trade(Instrument.LiteCoin);
            var client = new BtcRequestClient();
            var marketClient = new MarketClient(client);

            var trades = marketClient.GetTrades(Currencies.Aud, Instrument.Ethereum);

            foreach (var trade in trades)
            {
                Console.WriteLine($"Trade: ${trade.Price} x {trade.Amount} - {trade.Date} - {trade.Tid}");
            }

            Console.ReadKey();
        }

        public static void Trade(string instrument)
        {
            var client = new BtcRequestClient();
            var marketClient = new MarketClient(client);
            var orderClient = new OrdersClient(client);

            var edge = 1;

            var book = marketClient.GetOrderBook(Currencies.Btc, instrument);
            var lowestAsks = book.Asks
                .OrderBy(a => a[0])
                .First()[0];
            var askPrice = (long)(lowestAsks * TradingAdjustments.ApiPriceMultiplier) - edge;
            var highestBid = book.Bids
                .OrderByDescending(a => a[0])
                .First()[0];
            var bidPrice = (long)(highestBid * TradingAdjustments.ApiPriceMultiplier) + edge;

            var fee = 0.003;
            var sellPrice = bidPrice * (1 + fee);
            var purchasePrice = askPrice * (1 - fee);
            Console.WriteLine("=====================");
            Console.WriteLine($"Ask: {lowestAsks}");
            Console.WriteLine($"Bid: {highestBid}");
            Console.WriteLine($"Diff: {lowestAsks - highestBid}");
            Console.WriteLine("=====================");
            Console.WriteLine($"Buy: {purchasePrice}");
            Console.WriteLine($"Sell: {sellPrice}");
            Console.WriteLine($"Diff: {purchasePrice - sellPrice}");
            Console.WriteLine($"Profit %: {(purchasePrice - sellPrice) / sellPrice * 100}");
            var riskBuffer = 30000;
            if (purchasePrice - riskBuffer <= sellPrice)
            {
                Console.WriteLine("NO WAY RN'T BUYING NO NOTHING. I RN'T STUPID....");
                return;
            }

            //var askResponse = orderClient.CreateOrder(Currencies.Btc, instrument, askPrice, 100000, TradeSide.Ask);
            var askResponse = new CreateOrderResponse {Id = 595124578};
            var bidResponse = new CreateOrderResponse {Id = 596025673 };
            //var bidResponse = orderClient.CreateOrder(Currencies.Btc, instrument, bidPrice, 100000, TradeSide.Bid);

            OrderDetail[] orders;
            bool Checker()
            {
                var orderResponse = orderClient
                    .GetOrder(new[] {askResponse.Id, bidResponse.Id});
                orders = orderResponse.Orders;

                if (orders == null)
                {
                    Console.WriteLine($"They fucking with it: {orderResponse.ErrorMessage}");
                    Thread.Sleep(3000);
                    client = new BtcRequestClient();
                    return true;
                }

                return orders.Length != 2 ||
                       orders.Any(a => a.Status != "Fully Matched");
            }

            var lastBid = (long)1115926; // bidPrice;
            var lastAsk = (long)1194318; // askPrice;
            while (Checker())
            {
                if (orders == null)
                    continue;

                Thread.Sleep(300);

                var currentBook = marketClient.GetOrderBook(Currencies.Btc, instrument);
                var changed = false;

                var askOrder = orders.SingleOrDefault(a => a.OrderSide == TradeSide.Ask);
                if (askOrder != null && askOrder.Status != "Fully Matched")
                {
                    var currentLowestAsks = currentBook.Asks
                        .OrderBy(a => a[0])
                        .First()[0];
                    var currentAsk = (long)(currentLowestAsks * TradingAdjustments.ApiPriceMultiplier);
                    if (currentAsk < askPrice)
                    {
                        Console.WriteLine("Replacing Ask.");

                        if (currentAsk - edge < lastBid) return; // fuck...
                        var bidToCancel = askResponse.Id;
                        askResponse = orderClient.CreateOrder(Currencies.Btc, instrument, currentAsk - edge, askOrder.OpenVolume, TradeSide.Ask);
                        orderClient.CancelOrder(new[] { bidToCancel });
                        lastAsk = currentAsk - edge;
                        changed = true;
                    }
                }

                var bidOrder = orders.SingleOrDefault(a => a.OrderSide == TradeSide.Bid);
                if (bidOrder != null && bidOrder.Status != "Fully Matched")
                {
                    var currentHighestBid = currentBook.Bids
                        .OrderByDescending(a => a[0])
                        .First()[0];
                    var currentBidPrice = (long)(currentHighestBid * TradingAdjustments.ApiPriceMultiplier);
                    if (currentBidPrice > bidPrice)
                    {
                        Console.WriteLine("Replacing Bid.");
                        if (currentBidPrice + edge > lastAsk) return; // fuck...

                        var bidToCancel = bidResponse.Id;
                        bidResponse = orderClient.CreateOrder(Currencies.Btc, instrument, currentBidPrice + edge, bidOrder.OpenVolume, TradeSide.Bid);
                        orderClient.CancelOrder(new[] { bidToCancel });
                        lastBid = currentBidPrice + edge;
                        changed = true;
                    }
                }

                if (changed)
                {
                    var lastSellPrice = lastBid * (1 + fee);
                    var lastPurchasePrice = lastAsk * (1 - fee);
                    Console.WriteLine("=====================");
                    Console.WriteLine($"Buy: {lastPurchasePrice}");
                    Console.WriteLine($"Sell: {lastSellPrice}");
                    Console.WriteLine($"Diff: {lastPurchasePrice - lastSellPrice}");
                    Console.WriteLine($"Profit %: {(lastPurchasePrice - lastSellPrice) / lastSellPrice * 100}");
                    if (lastPurchasePrice - riskBuffer <= lastSellPrice)
                    {
                        Console.WriteLine("NO WAY RN'T BUYING NO NOTHING. I RN'T STUPID....");
                        orderClient.CancelOrder(new[] {bidResponse.Id, askResponse.Id});
                        return;
                    }
                }
            }
        }

        public static void ReportPrice()
        {
            var client = new BtcRequestClient();
            var marketClient = new MarketClient(client);

            //Market Data - GET requests (No Authentication Required)
            var instruments = new[]
            {
                Instrument.Ethereum,
                Instrument.BitcoinClassic,
                Instrument.BitcoinCash,
                Instrument.LiteCoin
            };
            var currencies = new[]
            {
                Currencies.Aud,
                Currencies.Btc
            };

            foreach (var instrument in instruments)
            {
                foreach (var currency in currencies
                    .Where(a => a != instrument))
                {
                    Console.WriteLine($"Currency: {currency} / Instrument: {instrument}");

                    var book = marketClient.GetOrderBook(currency, instrument);

                    var fee = currency == Currencies.Aud
                        ? 0.0060 : 0.0022;
                    var lowestAsks = book.Asks
                        .OrderBy(a => a[0])
                        .First()[0];
                    var highestBid = book.Bids
                        .OrderByDescending(a => a[0])
                        .First()[0];

                    var sellPrice = highestBid * (1 + fee);
                    var purchasePrice = lowestAsks * (1 - fee);

                    Console.WriteLine("=====================");
                    Console.WriteLine($"Ask: {lowestAsks}");
                    Console.WriteLine($"Bid: {highestBid}");
                    Console.WriteLine($"Diff: {lowestAsks - highestBid}");
                    Console.WriteLine("=====================");
                    Console.WriteLine($"Buy: {purchasePrice}");
                    Console.WriteLine($"Sell: {sellPrice}");
                    Console.WriteLine($"Diff: {purchasePrice - sellPrice}");
                    Console.WriteLine($"Profit %: {(purchasePrice - sellPrice) / sellPrice * 100}");
                    Console.WriteLine("");
                    Console.WriteLine("");
                }
            }
        }
    }
}
