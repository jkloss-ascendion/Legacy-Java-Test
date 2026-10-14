using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LegacyInventory.Core.Services
{
    public class PriceFeedClient
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;

        public PriceFeedClient(string baseUrl, HttpClient httpClient)
        {
            _baseUrl = baseUrl;
            _httpClient = httpClient;
        }

        public async Task<decimal?> GetLatestPriceAsync(string sku)
        {
            var requestUrl = _baseUrl + "/prices/" + Uri.EscapeDataString(sku);

            try
            {
                var response = await _httpClient.GetAsync(requestUrl).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var quote = JsonConvert.DeserializeObject<PriceQuote>(json);
                return quote == null ? (decimal?)null : quote.Price;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        private class PriceQuote
        {
            public string Sku { get; set; }

            public decimal Price { get; set; }
        }
    }
}
