/*
 *  OblivionAPI :: LookupService
 *
 *  This service is used to read data from external non-blockchain services.
 * 
 */

using System.Linq;

using CoinGecko.Clients;
using CoinGecko.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;

using Newtonsoft.Json;

namespace OblivionAPI.Services; 

public class LookupService {
    private readonly ILogger<LookupService> _logger;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ICoinGeckoClient _coinGecko;

    private readonly List<CachedPrice> _prices = new();
    private readonly List<HistoricalPrice> _priceHistory = new();

    private int _generalErrors;
    private int _previousGeneralErrors;
    private int _totalGeneralErrors;
    
    private int _ipfsTimeouts;
    private int _previousIpfsTimeouts;
    private int _totalIpfsTimeouts;
    
    private int _exceptions;
    private int _previousExceptions;
    private int _totalExceptions;

    public LookupService(ILogger<LookupService> logger, IHttpClientFactory httpFactory) {
        _logger = logger;
        _httpFactory = httpFactory;
        _coinGecko = CoinGeckoClient.Instance;
    }

    public async Task AddStatus(StringBuilder builder) {
        await Task.Run(() => {
            builder.AppendLine("Lookup Service Errors");
            builder.AppendLine("=====================");
            builder.AppendLine($"IPFS Timeouts  : {_ipfsTimeouts} | {_previousIpfsTimeouts} | {_totalIpfsTimeouts}");
            builder.AppendLine($"General Errors : {_generalErrors} | {_previousGeneralErrors} | {_totalGeneralErrors}");
            builder.AppendLine($"Exceptions     : {_exceptions} | {_previousExceptions} | {_totalExceptions}");
        });
    }

    public void ResetCounters() {
        _previousGeneralErrors = _generalErrors;
        _previousIpfsTimeouts = _ipfsTimeouts;
        _previousExceptions = _exceptions;
        _generalErrors = 0;
        _ipfsTimeouts = 0;
        _exceptions = 0;
    }

    public async Task<NftMetadataResponse> GetNFTMetadata(string uri) {
        try {
            _logger.LogDebug("Retrieving NFT metadata from: {Uri}", uri);
            // Ignore certain testnet URIs that are not valid anyways
            if (string.IsNullOrEmpty(uri) || uri.Contains("test.com") || uri.Contains("dawdsawddasws"))
                return null;
            uri = uri.Replace(Globals.IPFS_RAW_PREFIX, Globals.IPFS_HTTP_PREFIX);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var client = _httpFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode) {
                var options = new JsonSerializerOptions { AllowTrailingCommas = true };
                return await response.Content.ReadFromJsonAsync<NftMetadataResponse>(options);
            }

            _logger.LogError("Error during NFT metadata lookup from: {Uri}  {Error}", uri, response.ReasonPhrase);
            _generalErrors++;
            _totalGeneralErrors++;
            return null;
        }
        catch (System.Text.Json.JsonException error) {
            _logger.LogWarning(error, "JSON deserialization failed for {Uri}", uri);
            return null;
        }
        catch (TaskCanceledException error) {
            if (error.Message.Contains("The request was canceled due to the configured HttpClient.Timeout")) {
                _logger.LogWarning("IPFS timeout looking up {Uri}", uri);
                _ipfsTimeouts++;
                _totalIpfsTimeouts++;
            }
            else {
                _logger.LogError(error, "Exception during NFT metadata lookup from: {Uri}", uri);
                _exceptions++;
                _totalExceptions++;
            }
            
            return null;
        }
        catch (InvalidOperationException error) {
            if (error.Message.Contains("An invalid request URI was provided")) {
                _logger.LogWarning("An invalid URI provided for metadata lookup: {Uri}", uri);
                _generalErrors++;
                _totalGeneralErrors++;
            }
            else {
                _logger.LogError(error, "Exception during NFT metadata lookup from: {Uri}", uri);
                _exceptions++;
                _totalExceptions++;
            }

            return null;
        } 
        catch (Exception error) {
            _logger.LogError(error, "Exception during NFT metadata lookup from: {Uri}", uri);
            _exceptions++;
            _totalExceptions++;
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
            _exceptions++;
            _totalExceptions++;
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
            _exceptions++;
            _totalExceptions++;
            return 0;
        }
    }

    public async Task PinIPFSCids(List<string> cids) {
        try {
            _logger.LogDebug("Pinning IPFS CIDs");
            if (cids == null || !cids.Any()) return;
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IPFS_BEARER"))) {
                _logger.LogCritical("IPFS BEARER TOKEN NOT SET!!");
                return;
            }
            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("IPFS_BEARER"));
            var cidList = new CidList { cids = cids };
            var content = new StringContent(JsonConvert.SerializeObject(cidList), Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            _logger.LogDebug("IPFS CIDS: {Content}", await content.ReadAsStringAsync());
            var result = await client.PostAsync(Globals.IPFS_CID_POST_URL, content);
            if (!result.IsSuccessStatusCode) {
                _logger.LogWarning("IPFS pin CIDs update failed: {Code} : {Reason}", result.StatusCode, result.ReasonPhrase);
                _generalErrors++;
                _totalGeneralErrors++;
            }
        } catch (Exception error) {
            _logger.LogError(error, "An exception occured while pinning IPFS CIDs");
            _exceptions++;
            _totalExceptions++;
        }
    }

    public async Task UpdateNftApi(ChainID chainID, List<string> nfts) {
        try {
            _logger.LogDebug("Updating NFT API");
            var uri = Globals.Blockchains.Find(a => a.ChainID == chainID)?.NftApiUri;
            if (nfts == null || !nfts.Any() || string.IsNullOrEmpty(uri)) return;
            
            var client = _httpFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(nfts), Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var result = await client.PostAsync(uri, content);
            if (!result.IsSuccessStatusCode) {
                _logger.LogWarning("NFT API update failed: {Code} : {Reason}", result.StatusCode, result.ReasonPhrase);
                _generalErrors++;
                _totalGeneralErrors++;
            }
        } catch (Exception error) {
            _logger.LogError(error, "An exception occured while updating NFT API");
            _exceptions++;
            _totalExceptions++;
        }
    }
}