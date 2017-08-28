namespace BitClouded.TradeBot.DataAccess
{
    public class ApplicationConstants
    {
        public const string ApiKey = "";
        public const string PrivateKey = "";
    }

    public static class TradeSide
    {
        public const string Ask = "Ask";
        public const string Bid = "Bid";
    }

    public static class Currencies
    {
        public const string Usd = "USD";
        public const string Btc = "BTC";
        public const string Aud = "AUD";
    }

    public static class Instrument
    {
        public const string BitcoinClassic = "BTC";
        public const string LiteCoin = "LTC";
        public const string Ethereum = "ETH";
        public const string BitcoinCash = "BCH";
    }

    public static class TradingAdjustments
    {
        public const double BitCoinDiff = 0.00000001;
        public const double AudDiff = 0.01;
        public const long ApiPriceMultiplier = 100000000;
    }
}