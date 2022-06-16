﻿/*
 *  OblivionAPI :: DatabaseService
 *
 *  This service is used to store and manage the in-memory database.
 * 
 */

using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace OblivionAPI.Services; 

public class DatabaseService {
    public bool InitialSyncComplete;
    public bool DatabaseLoaded;
        
    private readonly BlockchainService _blockchain;
    private readonly LookupService _lookup;
    private readonly ImageCacheService _imageCache;
    private readonly ILogger<DatabaseService> _logger;

    private List<OblivionDetails> _details;
    private readonly DateTime _initializationTime;
    private DateTime _lastSyncCompleteTime;
    private DateTime _currentSyncStarted;
    private TimeSpan _lastSyncTime;
    private readonly Stopwatch _currentSyncTimer;
    private uint _totalScanSeconds;
    private int _totalScans;
    private bool _updateCancelling;
    private int _cancelledSyncs;
    private string _incompleteSyncs = "none";

    public DatabaseService(BlockchainService blockchain, LookupService lookup, ImageCacheService imageCache, ILogger<DatabaseService> logger) {
        _blockchain = blockchain;
        _lookup = lookup;
        _imageCache = imageCache;
        _logger = logger;
        _initializationTime = DateTime.Now;
        _currentSyncTimer = new Stopwatch();
        CheckDatabase();
    }

    private void CheckDatabase() {
        _details ??= new();
        
        if (_details.Find(a => a.ChainID == ChainID.BSC_Mainnet) == null) _details.Add(new BSCMainnetDefaults());
        if (_details.Find(a => a.ChainID == ChainID.BSC_Testnet) == null) _details.Add(new BSCTestnetDefaults());
        if (_details.Find(a => a.ChainID == ChainID.Nervos_Testnet) == null) _details.Add(new NervosTestnetDefaults());
        
        var checkOldNervosTestnet = _details.Find(a => a.ChainID == ChainID.Old_Nervos_Testnet);
        if (checkOldNervosTestnet != null) _details.Remove(checkOldNervosTestnet);

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CLEAR_DB"))) return;
        if (!long.TryParse(Environment.GetEnvironmentVariable("CLEAR_DB"), out var chainId)) return;
        
        switch (chainId) {
            case (long)ChainID.BSC_Mainnet:
                var bsc = _details.Find(a => a.ChainID == ChainID.BSC_Mainnet);
                if (bsc != null) _details.Remove(bsc);
                _details.Add(new BSCMainnetDefaults());
                break;
            case (long)ChainID.BSC_Testnet:
                var bscTestnet = _details.Find(a => a.ChainID == ChainID.BSC_Testnet);
                if (bscTestnet != null) _details.Remove(bscTestnet);
                _details.Add(new BSCTestnetDefaults());
                break;
            case (long)ChainID.Nervos_Testnet:
                var nervosTestnet = _details.Find(a => a.ChainID == ChainID.Nervos_Testnet);
                if (nervosTestnet != null) _details.Remove(nervosTestnet);
                _details.Add(new NervosTestnetDefaults());
                break;
        }
    }

    private bool CheckCancel() {
        if (_updateCancelling) return true;
        var syncTime = (DateTime.Now - _currentSyncStarted).TotalSeconds;
        
        switch (InitialSyncComplete) {
            case false when syncTime > Globals.MAX_INITIAL_SYNC_TIME:
            case true when syncTime > Globals.MAX_SYNC_TIME:
                _logger.LogCritical("Update loop being cancelled due to length of sync");
                _updateCancelling = true;
                _cancelledSyncs++;
                _incompleteSyncs = "";
                foreach (var chain in _details.Where(chain => !chain.LastSyncComplete)) {
                    _incompleteSyncs += $"{chain.ChainID} ";
                }
                break;
        }
        
        return _updateCancelling;
    }
    
    public async Task LoadDatabase() {
        DatabaseLoaded = true;
        try {
            if (!File.Exists(Globals.DB_FILE)) return;
            await using var stream = File.OpenRead(Globals.DB_FILE);
            _details = await JsonSerializer.DeserializeAsync<List<OblivionDetails>>(stream);
            await stream.DisposeAsync();
            CheckDatabase();
        } catch (Exception error) {
            _logger.LogError(error, "An exception occured while loading database file");
        }
    }

    private async Task SaveDatabase() {
        try {
            await using var stream = File.Create(Globals.DB_FILE);
            await JsonSerializer.SerializeAsync(stream, _details);
            await stream.DisposeAsync();
        } catch (Exception error) {
            _logger.LogError(error, "An exception occured while saving database file");
        }
    }

    public async Task<string> GetStatus() {
        long averageScan = 0;
        if (_totalScans > 0) averageScan = _totalScanSeconds / _totalScans;

        long databaseSize = 0;
        if (File.Exists(Globals.DB_FILE)) {
            var database = new FileInfo(Globals.DB_FILE);
            databaseSize = database.Length;
        }

        long imageCacheSize = 0;
        if (Directory.Exists(Globals.IMAGE_CACHE_DIR)) {
            var directory = new DirectoryInfo(Globals.IMAGE_CACHE_DIR);
            var files = directory.GetFiles();
            imageCacheSize += files.Sum(file => file.Length);
        }
        
        var status = new StringBuilder();
        status.AppendLine("API Status");
        status.AppendLine("");
        status.AppendLine($"Initialization Time            : {_initializationTime}");
        status.AppendLine($"Initial Sync Complete          : {InitialSyncComplete}");
        status.AppendLine($"Incomplete Syncs               : {_incompleteSyncs}");
        status.AppendLine("");
        status.AppendLine($"Current Sync Started           : {_currentSyncStarted}");
        status.AppendLine($"Current Sync Elapsed (seconds) : {(int) _currentSyncTimer.Elapsed.TotalSeconds}");
        status.AppendLine("");
        status.AppendLine($"Last Sync Complete             : {_lastSyncCompleteTime}");
        status.AppendLine($"Last Sync Time (seconds)       : {(int) _lastSyncTime.TotalSeconds}");
        status.AppendLine($"Average Sync Time (seconds)    : {(int) averageScan}");
        status.AppendLine($"Cancelled Syncs                : {_cancelledSyncs}");
        status.AppendLine("");
        status.AppendLine($"Database Size                  : {$"{databaseSize:n0}".PadLeft(15, ' ')} bytes");
        status.AppendLine($"Image Cache Size               : {$"{imageCacheSize:n0}".PadLeft(15, ' ')} bytes");
        status.AppendLine("");
        status.AppendLine("Errors Output As : CurrentHour | PreviousHour | Total");
        status.AppendLine("");

        await _blockchain.AddStatus(status);
        
        await _lookup.AddStatus(status);
        status.AppendLine("");

        await _imageCache.AddStatus(status);
        status.AppendLine("");
        
        foreach (var set in _details) {
            set.AddStatus(status);
            status.AppendLine("");
        }
        
        return status.ToString();
    }
    
    public async Task<uint> TotalListings(ChainID chainID) {
        return await Task.Run(() => {
            var details = _details.Find(a => a.ChainID == chainID);
            return details?.TotalListings ?? 0;
        });
    }
        
    public async Task<uint> TotalOffers(ChainID chainID, uint id, int version) {
        return await Task.Run(() => {
            var details = _details.Find(a => a.ChainID == chainID);
            var listing = details?.Listings.Find(a => a.ID == id && a.Version == version);
            return listing == null ? 0 : Convert.ToUInt32(listing.Offers.Count);
        });
    }
        
    public async Task<uint> TotalCollections(ChainID chainID) {
        return await Task.Run(() => {
            var details = _details.Find(a => a.ChainID == chainID);
            return details?.TotalCollections ?? 0;
        });
    }
        
    public async Task<uint> TotalReleases(ChainID chainID) {
        return await Task.Run(() => {
            var details = _details.Find(a => a.ChainID == chainID);
            return details?.TotalReleases ?? 0;
        });
    }
        
    public async Task<List<OblivionSaleInformation>> GetSales(ChainID chainID) {
        return await Task.Run(() => {
            var details = _details.Find(a => a.ChainID == chainID);
            var listings = details?.Listings.Where(a => a.WasSold);
            return listings?.Select(listing => listing.SaleInformation).ToList();
        });
    }

    public async Task<List<ReleaseSaleDetails>> GetReleaseSales(ChainID chainID) {
        return await Task.Run(() => {
            var details = _details.Find(a => a.ChainID == chainID);
            return details?.ReleaseSales.ToList();
        });
    }

    public async Task<List<ListListingsDTO>> GetListings(ChainID chainID) {
        return await Task.Run(() => {
            var details = _details.Find(a => a.ChainID == chainID);
            return details == null ? null : (from listing in details.Listings let nft = details.NFTs.Find(a => a.Address == listing.NFT) select new ListListingsDTO(listing, nft)).ToList();
        });
    }

    public async Task<List<NFTDetails>> GetNFTs(ChainID chainID) {
        return await Task.Run(() => {
            var details = _details.Find(a => a.ChainID == chainID);
            return details?.NFTs.ToList();
        });
    }

    public async Task<List<OfferDetails>> GetOffers(ChainID chainID, uint id, int version) {
        return await Task.Run(() => {
            var details = _details.Find(a => a.ChainID == chainID);
            var listing = details?.Listings.Find(a => a.ID == id && a.Version == version);
            return listing?.Offers.ToList();
        });
    }

    public async Task<List<OfferDetails>> GetUserOffers(ChainID chainID, string wallet) {
        return await Task.Run(() => {
            var details = _details.Find(a => a.ChainID == chainID);
            return details?.Listings.SelectMany(listing => listing.Offers.Where(a => a.Offeror == wallet)).ToList();
        });
    }
        
    public async Task<List<CollectionDetails>> GetCollections(ChainID chainID) {
        return await Task.Run(() => {
            var details = _details.Find(a => a.ChainID == chainID);
            return details?.Collections.ToList();
        });
    }
        
    public async Task<List<ReleaseDetails>> GetReleases(ChainID chainID) {
        return await Task.Run(() => {
            var details = _details.Find(a => a.ChainID == chainID);
            return details?.Releases.ToList();
        });
    }
        
    public static async Task<List<PaymentTokenDetails>> GetPaymentTokens(ChainID chainID) {
        return await Task.Run(() => {
            var tokens = Globals.Payments.Find(a => a.ChainID == chainID);
            return tokens?.PaymentTokens;
        });
    }

    public async Task<ListingDetails> ListingDetails(ChainID chainID, int version, uint id) {
        return await RetrieveListing(chainID, version, id, false);
    }

    public async Task<OfferDetails> OfferDetails(ChainID chainID, int version, uint id, string paymentToken, uint offerID) {
        return await RetrieveOffer(chainID, version, id, paymentToken, offerID, false);
    }

    public async Task<CollectionDetails> CollectionDetails(ChainID chainID, uint id) {
        return await RetrieveCollection(chainID, id, false);
    }

    public async Task<NFTDetails> NFTDetails(ChainID chainID, string address) {
        return await RetrieveNFT(chainID, address, false);
    }

    public async Task<NFTTokenIDInfo> NFTTokenURI(ChainID chainID, string address, uint tokenID) {
        return await RetrieveNFTTokenURI(chainID, address, tokenID, false);
    }

    public async Task<ReleaseDetails> ReleaseDetails(ChainID chainID, uint id) {
        return await RetrieveRelease(chainID, id, false);
    }
        
    public async Task<ListingDetails> RefreshListing(ChainID chainID, int version, uint id) {
        var stopwatch = Stopwatch.StartNew();
        var listing = await RetrieveListing(chainID, version, id, true);
        if (listing == null) return null;
        await UpdateListingExtraDetails(chainID, listing);
        listing = await RetrieveListing(chainID, version, id, false);
        stopwatch.Stop();
        _logger.LogDebug("Refresh listing took {Milliseconds} ms", stopwatch.ElapsedMilliseconds);
        return listing;
    }

    public async Task<OfferDetails> RefreshOffer(ChainID chainID, int version, uint id, string paymentToken, uint offerID) {
        return await RetrieveOffer(chainID, version, id, paymentToken, offerID, true);
    }

    public async Task<CollectionDetails> RefreshCollection(ChainID chainID, uint id) {
        return await RetrieveCollection(chainID, id, true);
    }

    public async Task<ReleaseDetails> RefreshRelease(ChainID chainID, uint id) {
        return await RetrieveRelease(chainID, id, true);
    }

    public async Task<NFTDetails> RefreshNft(ChainID chainID, string address) {
        return await RetrieveNFT(chainID, address, true);
    }

    public async Task<NFTDetails> RecacheNft(ChainID chainID, string address) {
        return await RetrieveNFT(chainID, address, true, true);
    }

    public async Task HandleUpdate() {
        _updateCancelling = false;
        _currentSyncStarted = DateTime.Now;
        _currentSyncTimer.Start();
        foreach (var set in _details) set.ClearStatus();
        var tasks = _details.Select(set => Task.Run(async () => await HandleChainUpdate(set))).ToList();
        var run = Task.WhenAll(tasks);
        await run.WaitAsync(new CancellationToken());
        await SaveDatabase();
        _currentSyncTimer.Stop();
        InitialSyncComplete = true;
        _lastSyncCompleteTime = DateTime.Now;
        _lastSyncTime = _currentSyncTimer.Elapsed;
        _currentSyncStarted = DateTime.MinValue;
        _totalScans++;
        _totalScanSeconds += (uint) _currentSyncTimer.Elapsed.TotalSeconds;
        _currentSyncTimer.Reset();
        if (!_updateCancelling) _incompleteSyncs = "none";
    }

    private async Task HandleChainUpdate(OblivionDetails set) {
        var stopwatch = Stopwatch.StartNew();
        await UpdateBasicDetails(set.ChainID);
        await UpdateListings(set);
        await UpdateCollections(set);
        await UpdateReleases(set);
        await UpdateTokens(set.ChainID);
        await UpdateSaleCollections(set);
        await UpdateReleaseSales(set);
        await UpdateListingCollections(set.ChainID);
        await UpdateIPFS(set.ChainID);
        _details.Find(a => a.ChainID == set.ChainID)!.LastSyncTime = (int) stopwatch.Elapsed.TotalSeconds;
        _details.Find(a => a.ChainID == set.ChainID)!.LastSyncComplete = true;
    }
        
    private async Task UpdateBasicDetails(ChainID chainID) {
        var details = _details.Find(a => a.ChainID == chainID);
        if (details == null) return;

        details.TotalListingsV1 = await _blockchain.GetTotalListings(details.ChainID, 1);
        details.TotalListingsV2 = await _blockchain.GetTotalListings(details.ChainID, 2);
        details.TotalListings = details.TotalListingsV1 + details.TotalListingsV2;
        details.TotalCollections = await _blockchain.GetTotalCollections(details.ChainID);
        details.TotalReleases = await _blockchain.GetTotalReleases(details.ChainID);
    }

    private async Task UpdateListings(OblivionDetails set) {
        foreach (var listing in set.Listings.Where(a => !a.Finalized)) {
            if (CheckCancel()) return;
            await RetrieveListing(set.ChainID, listing.Version, listing.ID, true);
        }

        for (var id = set.Listings.Count(a => a.Version == 1); id < set.TotalListingsV1; id++) {
            if (CheckCancel()) return;
            await RetrieveListing(set.ChainID, 1, Convert.ToUInt32(id), true);
        }

        for (var id = set.Listings.Count(a => a.Version == 2); id < set.TotalListingsV2; id++) {
            if (CheckCancel()) return;
            await RetrieveListing(set.ChainID, 2, Convert.ToUInt32(id), true);
        }

        foreach (var listing in set.Listings.Where(a => !a.Finalized)) {
            if (CheckCancel()) return;
            var checkNft = set.NFTs.Find(a => a.Address == listing.NFT) ?? await RetrieveNFT(set.ChainID, listing.NFT, false);

            if (checkNft != null) {
                var checkToken = checkNft.TokenDetails.Find(a => a.TokenId == listing.TokenId);
                if (checkToken == null) await RetrieveNFTTokenURI(set.ChainID, listing.NFT, listing.TokenId, false);
            }

            await UpdateListingExtraDetails(set.ChainID, listing);
        }

        _details.Find(a => a.ChainID == set.ChainID)!.ListingsUpdated = true;
    }

    private async Task UpdateListingExtraDetails(ChainID chainId, ListingDetails listing) {
        var payments = Globals.Payments.Find(a => a.ChainID == chainId);
        if (payments == null) return;

        decimal topOfferValue = 0;
        OfferDetails topOffer = null;
                
        foreach (var token in payments.PaymentTokens) {
            foreach (var offer in listing.Offers.Where(a => a.PaymentToken == token.Address && !a.Claimed)) {
                if (CheckCancel()) return;
                var check = await RetrieveOffer(chainId, listing.Version, listing.ID, token.Address, offer.ID, true);
                if (check.Claimed && listing.SaleState == 0) continue;
                var value = await ConvertTokensToUSD(BigInteger.Parse(check.Amount), token.Decimals, token.CoinGeckoKey);
                if (value > topOfferValue) {
                    topOffer = check;
                    topOfferValue = value;
                }
            }
                    
            var total = await _blockchain.GetListingOffers(chainId, listing.Version, listing.ID, token.Address);
            for (var id = listing.Offers.Count(a => a.PaymentToken == token.Address); id < total; id++) {
                if (CheckCancel()) return;
                var check = await RetrieveOffer(chainId, listing.Version, listing.ID, token.Address, Convert.ToUInt32(id), true);
                if (check.Claimed && listing.SaleState == 0) continue;
                var value = await ConvertTokensToUSD(BigInteger.Parse(check.Amount), token.Decimals, token.CoinGeckoKey);
                if (value > topOfferValue) {
                    topOffer = check;
                    topOfferValue = value;
                }
            }
        }

        listing.TopOffer = topOffer;

        if (listing.SaleState != 0 && !listing.Finalized) await FinalizeListing(chainId, listing.Version, listing);
    }

    private async Task UpdateCollections(OblivionDetails set) {
        foreach (var collection in set.Collections) {
            if (CheckCancel()) return;
            await RetrieveCollection(set.ChainID, collection.ID, true);
        }

        for (var id = set.Collections.Count; id < set.TotalCollections; id++) {
            if (CheckCancel()) return;
            await RetrieveCollection(set.ChainID, Convert.ToUInt32(id), true);
        }

        var collections = _details.Find(a => a.ChainID == set.ChainID)?.Collections.ToList();
        if (collections == null) return;

        foreach (var nft in collections.SelectMany(collection => collection.Nfts)) {
            if (CheckCancel()) return;
            await RetrieveNFT(set.ChainID, nft, false);
        }
        
        _details.Find(a => a.ChainID == set.ChainID)!.CollectionsUpdated = true;
    }

    private async Task UpdateReleases(OblivionDetails set) {
        foreach (var release in set.Releases.Where(a => !a.Ended)) {
            if (CheckCancel()) return;
            await RetrieveRelease(set.ChainID, release.ID, true);
        }

        for (var id = set.Releases.Count; id < set.TotalReleases; id++) {
            if (CheckCancel()) return;
            await RetrieveRelease(set.ChainID, Convert.ToUInt32(id), true);
        }

        var releases = _details.Find(a => a.ChainID == set.ChainID)?.Releases.ToList();
        if (releases == null) return;

        foreach (var release in releases) {
            if (CheckCancel()) return;
            await RetrieveNFT(set.ChainID, release.NFT, false);
        }
        
        _details.Find(a => a.ChainID == set.ChainID)!.ReleasesUpdated = true;
    }

    private async Task FinalizeListing(ChainID chainID, int version, ListingDetails listing) {
        var sale = await _blockchain.CheckSale(chainID, version, listing);
        if (sale == null) return;
        listing.Finalized = true;
        listing.TxHash = sale.TxHash;
        if (!sale.Cancelled) {
            sale.Nft = listing.NFT;
            listing.WasSold = true;
            listing.SaleInformation = sale;
        }
    }

    private async Task UpdateSaleCollections(OblivionDetails set) {
        await Task.Run(() => {
            var sales = set.Listings.Where(a => a.WasSold);
            foreach (var sale in sales) {
                if (CheckCancel()) return;
                var collection = set.Collections.Find(a => a.Nfts.Contains(sale.NFT));
                if (collection != null) sale.SaleInformation.CollectionId = collection.ID;
                else sale.SaleInformation.CollectionId = null;
            }
            _details.Find(a => a.ChainID == set.ChainID)!.SaleCollectionsUpdated = true;
        });
    }

    private async Task UpdateTokens(ChainID chainID) {
        var payments = Globals.Payments.Find(a => a.ChainID == chainID);
        if (payments == null) return;

        foreach (var token in payments.PaymentTokens) {
            if (CheckCancel()) return;
            token.Price = await _lookup.GetCurrentPrice(token.CoinGeckoKey);
        }
        
        _details.Find(a => a.ChainID == chainID)!.TokensUpdated = true;
    }

    private async Task UpdateReleaseSales(OblivionDetails set) {
        if (set.ReleaseStartingBlock == 0) return;
        if (set.LastReleaseScannedBlock == 0) set.LastReleaseScannedBlock = set.ReleaseStartingBlock - 1;

        var scannedBlocks = 0;
        var lastBlock = await _blockchain.GetLatestBlock(set.ChainID);
        var blocksToScan = lastBlock - set.LastReleaseScannedBlock;

        while (blocksToScan > 0 && scannedBlocks < Globals.MAX_BLOCKS_SCANNED_PER_UPDATE) {
            if (CheckCancel()) return;
            
            if (set.LastReleaseScannedBlock > lastBlock) {
                set.LastReleaseScannedBlock = lastBlock;
                break;
            }
            
            var start = set.LastReleaseScannedBlock;
            uint end;

            if (blocksToScan > 5000) {
                end = start + 5000;
                blocksToScan -= 5000;
                scannedBlocks += 5000;
            }
            else {
                end = start + blocksToScan;
                blocksToScan = 0;
                scannedBlocks += (int) blocksToScan;
            }

            var sales = await _blockchain.CheckReleaseSales(set.ChainID, start, end);
            set.LastReleaseScannedBlock = end;
                
            if (sales == null) break;
            if (sales.Count == 0) continue;

            foreach (var sale in sales) {
                var release = set.Releases.Find(a => a.ID == sale.ID);
                sale.Price = release?.Price;
                sale.PaymentToken = release?.PaymentToken;
                set.ReleaseSales.Add(sale);
            }
            
            lastBlock = await _blockchain.GetLatestBlock(set.ChainID);
        }
        
        _details.Find(a => a.ChainID == set.ChainID)!.ReleaseSalesUpdated = true;
    }

    private async Task UpdateListingCollections(ChainID chainID) {
        await Task.Run(() => {
            var details = _details.Find(a => a.ChainID == chainID);
            if (details == null) return;
            foreach (var listing in details.Listings.Where(a => !a.Finalized || !InitialSyncComplete)) {
                if (CheckCancel()) return;
                var collection = details.Collections.Find(a => a.Nfts.Contains(listing.NFT));
                if (collection != null) {
                    listing.CollectionId = collection.ID;
                    listing.CollectionName = collection.Name;
                } else {
                    listing.CollectionId = null;
                    listing.CollectionName = null;
                }
                
            }
            
            _details.Find(a => a.ChainID == chainID)!.ListingCollectionsUpdated = true;
        });
    }

    private async Task UpdateIPFS(ChainID chainId) {
        var details = _details.Find(a => a.ChainID == chainId);
        if (details == null) return;
        var cids = (from nft in details.NFTs where nft.Metadata?.Image != null where nft.Metadata.Image.StartsWith(Globals.IPFS_RAW_PREFIX) select nft.Metadata.Image.Remove(0, Globals.IPFS_RAW_PREFIX.Length)).ToList();
        await _lookup.PinIPFSCids(cids);
        _details.Find(a => a.ChainID == chainId)!.IPFSUpdated = true;
    }

    private async Task<ListingDetails> RetrieveListing(ChainID chainID, int version, uint id, bool forceUpdate) {
        var details = _details.Find(a => a.ChainID == chainID);
        if (details == null) return null;
            
        var listing = details.Listings.Find(a => a.ID == id && a.Version == version);

        var fresh = false;
            
        if (listing == null) {
            listing = await _blockchain.GetListing(chainID, version, id);
            if (listing == null) return null;
            details.Listings.Add(listing);
            fresh = true;
        }

        if (DateTime.Now - listing.LastRetrieved > TimeSpan.FromMinutes(Globals.CACHE_TIME) || (forceUpdate && !fresh)) {
            var result = await _blockchain.GetListing(chainID, version, id);
            listing.Update(result);
        }
            
        return listing;
    }

    private async Task<OfferDetails> RetrieveOffer(ChainID chainID, int version, uint id, string paymentToken, uint offerID, bool forceUpdate) {
        var details = _details.Find(a => a.ChainID == chainID);
        var listing = details?.Listings.Find(a => a.ID == id && a.Version == version);
        if (listing == null) return null;

        var fresh = false;
            
        var offer = listing.Offers.Find(a => a.PaymentToken == paymentToken && a.ID == offerID);
        if (offer == null) {
            offer = await _blockchain.GetOffer(chainID, version, id, paymentToken, offerID);
            if (offer == null) return null;
            offer.TokenId = listing.TokenId;

            var nft = await RetrieveNFT(chainID, listing.NFT, false);
            if (nft != null) offer.NftSymbol = nft.Symbol;
                
            listing.Offers.Add(offer);
            fresh = true;
        }

        if (DateTime.Now - offer.LastRetrieved > TimeSpan.FromMinutes(Globals.CACHE_TIME) || (forceUpdate && !fresh)) {
            var result = await _blockchain.GetOffer(chainID, version, id, paymentToken, offerID);
            offer.Update(result);
        }

        return offer;
    }

    private async Task<CollectionDetails> RetrieveCollection(ChainID chainID, uint id, bool forceUpdate) {
        var details = _details.Find(a => a.ChainID == chainID);
        if (details == null) return null;

        var fresh = false;
            
        var collection = details.Collections.Find(a => a.ID == id);
        if (collection == null) {
            collection = await _blockchain.GetCollection(chainID, id);
            if (collection == null) return null;
            details.Collections.Add(collection);
            fresh = true;
        }

        if (DateTime.Now - collection.LastRetrieved > TimeSpan.FromMinutes(Globals.CACHE_TIME) || (forceUpdate && !fresh)) {
            var update = await _blockchain.GetCollection(chainID, id);
            if (update != null) collection.Update(update);
        }

        return collection;
    }
        
    private async Task<NFTDetails> RetrieveNFT(ChainID chainID, string address, bool forceUpdate, bool reCache = false) {
        var details = _details.Find(a => a.ChainID == chainID);
        if (details == null) return null;

        var nft = details.NFTs.Find(a => a.Address == address);
        var addNeeded = nft == null;
        if (nft == null || forceUpdate) {
            nft = await _blockchain.GetNFTDetails(chainID, address);
            if (nft != null && addNeeded) details.NFTs.Add(nft);
        }

        if (nft == null) return null;

        if (nft is { Metadata: null } || !nft.CacheHighRes.StartsWith(Globals.IMAGE_CACHE_PREFIX) || forceUpdate) {
            var metadata = await _lookup.GetNFTMetadata(nft.URI ?? nft.BaseURI);
            if (metadata != null) {
                nft.Metadata = new NFTMetadata(metadata);
                var cache = await _imageCache.ImageCache(chainID, address, nft.Metadata.Image, 1, reCache);
                nft.CacheHighRes = !string.IsNullOrEmpty(cache.HighResImage) ? cache.HighResImage : nft.Metadata.Image;
                if (!string.IsNullOrEmpty(cache.LowResImage)) nft.CacheLowRes = cache.LowResImage;
                else nft.CacheLowRes = !string.IsNullOrEmpty(cache.HighResImage) ? cache.HighResImage : nft.Metadata.Image;
            }
        }

        return nft;
    }
        
    private async Task<NFTTokenIDInfo> RetrieveNFTTokenURI(ChainID chainID, string address, uint tokenID, bool forceUpdate, bool reCache = false) {
        var details = _details.Find(a => a.ChainID == chainID);
        if (details == null) return null;

        var nft = details.NFTs.Find(a => a.Address == address);
            
        if (nft == null) {
            nft = await RetrieveNFT(chainID, address, false);
            if (nft == null) return null;
        }

        var token = nft.TokenDetails.Find(a => a.TokenId == tokenID);
        if (token == null) {
            var uri = await _blockchain.GetNFTTokenURI(chainID, address, tokenID);
            if (uri != null) {
                token = new NFTTokenIDInfo { TokenId = tokenID, URI = uri };
                nft.TokenDetails.Add(token);
            }
        }

        if (token == null) return null;

        if (token is { Metadata: null } || !token.CacheHighRes.StartsWith(Globals.IMAGE_CACHE_PREFIX) || forceUpdate) {
            var metadata = await _lookup.GetNFTMetadata(token.URI);
            if (metadata != null) {
                token.Metadata = new NFTMetadata(metadata);
                if (nft.Metadata?.Image != token.Metadata?.Image && token.Metadata != null) {
                    var cache = await _imageCache.ImageCache(chainID, address, token.Metadata.Image, token.TokenId, reCache);
                    token.CacheHighRes = !string.IsNullOrEmpty(cache.HighResImage) ? cache.HighResImage : nft.CacheHighRes;
                    if (!string.IsNullOrEmpty(cache.LowResImage)) token.CacheLowRes = cache.LowResImage;
                    else token.CacheLowRes = !string.IsNullOrEmpty(cache.HighResImage) ? cache.HighResImage : nft.CacheLowRes;
                } else {
                    token.CacheLowRes = nft.CacheLowRes;
                    token.CacheHighRes = nft.CacheHighRes;
                }
            }
        }

        return token;
    }
        
    private async Task<ReleaseDetails> RetrieveRelease(ChainID chainID, uint id, bool forceUpdate) {
        var details = _details.Find(a => a.ChainID == chainID);
        if (details == null) return null;

        var fresh = false;
            
        var release = details.Releases.Find(a => a.ID == id);
        if (release == null) {
            release = await _blockchain.GetRelease(chainID, id);
            if (release == null) return null;
            details.Releases.Add(release);
            fresh = true;
        }

        if (DateTime.Now - release.LastRetrieved > TimeSpan.FromMinutes(Globals.CACHE_TIME) || (forceUpdate && !fresh)) {
            var update = await _blockchain.GetRelease(chainID, id);
            if (update != null) release.Update(update);
        }

        return release;
    }
        
    private async Task<decimal> ConvertTokensToUSD(BigInteger tokens, int decimals, string coinGeckoKey, DateTime? date = null) {
        var ratio = BigInteger.Pow(10, decimals);

        var tokenAmount = (double)tokens / (double)ratio;
        decimal price;
        if (date == null) price = await _lookup.GetCurrentPrice(coinGeckoKey);
        else price = await _lookup.GetHistoricalPrice(coinGeckoKey, date);

        return (decimal)tokenAmount * price;
    }
}