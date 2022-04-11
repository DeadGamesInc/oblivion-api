/*
 *  OblivionAPI :: LookupService
 *
 *  This service is used to read data from external non-blockchain services.
 * 
 */

using CoinGecko.Clients;
using CoinGecko.Interfaces;
using Microsoft.Extensions.Logging;
using OblivionAPI.Config;
using OblivionAPI.Objects;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace OblivionAPI.Services {
    public class LookupService {
        private readonly ILogger<LookupService> _logger;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ICoinGeckoClient _coinGecko;

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

                if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<NftMetadataResponse>();

                _logger.LogError("Error during NFT metadata lookup from: {Uri}  {Error}", uri, response.ReasonPhrase);
                return null;
            } catch (Exception error) {
                _logger.LogError(error, "Exception during NFT metadata lookup from: {Uri}", uri);
                return null;
            }
        }

        public async Task<decimal> GetCurrentPrice(string id) {
            try {
                _logger.LogInformation("Looking up token price for {ID}", id);
                if (string.IsNullOrEmpty(id)) return 0;
                var result = await _coinGecko.SimpleClient.GetSimplePrice(new[] { id }, new[] { "usd" });
                return result[id]["usd"] ?? 0;
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured while retrieving crypto price for {ID}", id);
                return 0;
            }
        }

        // Date is in format DD-MM-YYYY
        public async Task<decimal> GetHistoricalPrice(string id, string date) {
            try {
                _logger.LogInformation("Looking up price for {ID} on {Date}", id, date);
                var result = await _coinGecko.CoinsClient.GetHistoryByCoinId(id, date, "false");
                return result.MarketData.CurrentPrice["usd"] ?? 0;
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured while retrieving historical price for {ID}", id);
                return 0;
            }
        }
    }
}
