using System;
using System.Configuration;
using System.Net;
using Newtonsoft.Json;

namespace LegacyInventory.Core.Services
{
    public class PriceFeedClient
    {
        private readonly string _baseUrl;

        public PriceFeedClient()
        {
            _baseUrl = ConfigurationManager.AppSettings["PriceFeedUrl"];
        }

        public decimal? GetLatestPrice(string sku)
        {
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.Accept] = "application/json";

                try
                {
                    var json = client.DownloadString(_baseUrl + "/prices/" + Uri.EscapeDataString(sku));
                    var quote = JsonConvert.DeserializeObject<PriceQuote>(json);
                    return quote == null ? (decimal?)null : quote.Price;
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }

        private class PriceQuote
        {
            public string Sku { get; set; }

            public decimal Price { get; set; }
        }
    }
}
