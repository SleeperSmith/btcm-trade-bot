using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace BitClouded.TradeBot.DataAccess.Tests
{
    [TestFixture]
    public class TestSignatureFixture
    {
        [Test]
        public void SignatureIsValid()
        {
            var url = "/order/history";
            var timestamp = "1378818710123";
            var data = "{\"currency\":\"AUD\", \"instrument\":\"BTC\", \"limit\":\"10\"}";
            var stringToSign = BtcRequestClient.BuildStringToSign(
                url, data, timestamp);
            var signature = BtcRequestClient.ComputeHash(ApplicationConstants.PrivateKey, stringToSign);

            //Assert.AreEqual(signature, "bEDtDJnW0y/Ll4YZitxb+D5sTNnEpQKH67EJRCmQCqN9cvGiB8+IHzB7HjsOs3mSlxLmu4aiPDRpe9anuWzylw==");
        }
    }
}
