/*
 *  OblivionAPI :: LookupService
 *
 *  This service is used to read data from external non-blockchain services.
 * 
 */

using CoinGecko.Clients;
using CoinGecko.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;

namespace OblivionAPI.Services; 

public class LookupService {
    private readonly ILogger<LookupService> _logger;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ICoinGeckoClient _coinGecko;

    private readonly List<CachedPrice> _prices = new();
    private readonly List<HistoricalPrice> _priceHistory = new();

    public LookupService(ILogger<LookupService> logger, IHttpClientFactory httpFactory) {
        _logger = logger;
        _httpFactory = httpFactory;
        _coinGecko = CoinGeckoClient.Instance;
    }

    public async Task<NftMetadataResponse> GetNFTMetadata(string uri) {
        try {
            _logger.LogDebug("Retrieving NFT metadata from: {Uri}", uri);
            if (uri.Contains("test.com")) return null; // Ignore certain testnet URIs that are not valid anyways
            uri = uri.Replace(Globals.IPFS_RAW_PREFIX, Globals.IPFS_HTTP_PREFIX);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var client = _httpFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode) {
                var options = new JsonSerializerOptions { AllowTrailingCommas = true };
                return await response.Content.ReadFromJsonAsync<NftMetadataResponse>(options);
            }

            _logger.LogError("Error during NFT metadata lookup from: {Uri}  {Error}", uri, response.ReasonPhrase);
            return null;
        } catch (Exception error) {
            _logger.LogError(error, "Exception during NFT metadata lookup from: {Uri}", uri);
            return null;
        }
    }

    public async Task<decimal> GetCurrentPrice(string id) {
        try {
            _logger.LogDebug("Looking up token price for {ID}", id);
                
            if (string.IsNullOrEmpty(id)) return 0;
            var check = _prices.Find(a => a.CoinGeckoKey == id);
            if (DateTime.Now - check?.LastRetrieved < TimeSpan.FromMinutes(Globals.CACHE_TIME)) return check.Price;
                
            var result = await _coinGecko.SimpleClient.GetSimplePrice(new[] { id }, new[] { "usd" });
            if (check == null) {
                check = new CachedPrice { CoinGeckoKey = id };
                _prices.Add(check);
            }

            check.Price = result[id]["usd"] ?? 0;
            check.LastRetrieved = DateTime.Now;
                
            return check.Price;
        } catch (Exception error) {
            _logger.LogError(error, "An exception occured while retrieving crypto price for {ID}", id);
            return 0;
        }
    }

    // Date is in format DD-MM-YYYY
    public async Task<decimal> GetHistoricalPrice(string id, DateTime? dateTime) {
        try {
            if (dateTime == null) return 0;
            var date = dateTime?.ToString("dd-MM-yyyy");

            var check = _priceHistory.Find(a => a.CoinGeckoKey == id && a.PriceDate == date);
            if (check != null) return check.Price;

            Thread.Sleep(Globals.COIN_GECKO_THROTTLE_WAIT);
            _logger.LogInformation("Looking up price for {ID} on {Date}", id, date);
                
            var result = await _coinGecko.CoinsClient.GetHistoryByCoinId(id, date, "false");

            var price = new HistoricalPrice { CoinGeckoKey = id, Price = result.MarketData.CurrentPrice["usd"] ?? 0, PriceDate = date };
            _priceHistory.Add(price);
                
            return price.Price;
        } catch (Exception error) {
            _logger.LogError(error, "An exception occured while retrieving historical price for {ID}", id);
            return 0;
        }
    }
}