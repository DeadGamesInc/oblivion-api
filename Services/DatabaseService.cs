/*
 *  OblivionAPI :: DatabaseService
 *
 *  This service is used to store and manage the in-memory database.
 * 
 */

using OblivionAPI.Config;
using OblivionAPI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OblivionAPI.Services {
    public class DatabaseService {
        private readonly BlockchainService _blockchain;
        private readonly LookupService _lookup;
        private readonly ImageCacheService _imageCache;

        private readonly List<OblivionDetails> _details;

        public DatabaseService(BlockchainService blockchain, LookupService lookup, ImageCacheService imageCache) {
            _blockchain = blockchain;
            _lookup = lookup;
            _imageCache = imageCache;
            
            _details = new List<OblivionDetails> {
                new() { ChainID = ChainID.BSC_Mainnet, ReleaseStartingBlock = 16636640 },
                new() { ChainID = ChainID.BSC_Testnet, ReleaseStartingBlock = 17931172 },
                new() { ChainID = ChainID.Nervos_Testnet }
            };
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

        public async Task<List<ListingDetails>> GetListings(ChainID chainID) {
            return await Task.Run(() => {
                var details = _details.Find(a => a.ChainID == chainID);
                return details?.Listings.ToList();
            });
        }

        public async Task<List<OfferDetails>> GetOffers(ChainID chainID, uint id, int version) {
            return await Task.Run(() => {
                var details = _details.Find(a => a.ChainID == chainID);
                var listing = details?.Listings.Find(a => a.ID == id && a.Version == version);
                return listing?.Offers.ToList();
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
        
        public async Task<List<PaymentTokenDetails>> GetPaymentTokens(ChainID chainID) {
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
            return await RetrieveNFTTokenURI(chainID, address, tokenID);
        }

        public async Task<ReleaseDetails> ReleaseDetails(ChainID chainID, uint id) {
            return await RetrieveRelease(chainID, id, false);
        }
        
        public async Task<ListingDetails> RefreshListing(ChainID chainID, int version, uint id) {
            return await RetrieveListing(chainID, version, id, true);
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

        public async Task HandleUpdate() {
            var details = _details.ToList();

            foreach (var set in details) {
                await UpdateBasicDetails(set.ChainID);
                await UpdateListings(set);
                await UpdateCollections(set);
                await UpdateReleases(set);
                await UpdateTokens(set.ChainID);
                await UpdateSaleCollections(set);
                await UpdateReleaseSales(set);
            }
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
            foreach (var listing in set.Listings.Where(a => !a.Finalized)) await RetrieveListing(set.ChainID, listing.Version, listing.ID, true);
            
            for (var id = set.Listings.Count(a => a.Version == 1); id < set.TotalListingsV1; id++) await RetrieveListing(set.ChainID, 1, Convert.ToUInt32(id), true);
            for (var id = set.Listings.Count(a => a.Version == 2); id < set.TotalListingsV2; id++) await RetrieveListing(set.ChainID, 2, Convert.ToUInt32(id), true);

            foreach (var listing in set.Listings.Where(a => !a.Finalized)) {
                var checkNft = set.NFTs.Find(a => a.Address == listing.NFT);
                if (checkNft == null) await RetrieveNFT(set.ChainID, listing.NFT, false);
                
                var payments = Globals.Payments.Find(a => a.ChainID == set.ChainID);
                if (payments == null) continue;
                
                foreach (var token in payments.PaymentTokens) {
                    foreach (var offer in listing.Offers.Where(a => a.PaymentToken == token.Address && !a.Claimed)) 
                        await RetrieveOffer(set.ChainID, listing.Version, listing.ID, token.Address, offer.ID, true);
                    
                    var total = await _blockchain.GetListingOffers(set.ChainID, listing.Version, listing.ID, token.Address);
                    for (var id = listing.Offers.Count(a => a.PaymentToken == token.Address); id < total; id++)
                        await RetrieveOffer(set.ChainID, listing.Version, listing.ID, token.Address, Convert.ToUInt32(id), true);
                }
            }
            
            foreach (var listing in set.Listings.Where(a => !a.Finalized && a.SaleState != 0)) await FinalizeListing(set.ChainID, listing.Version, listing);
        }

        private async Task UpdateCollections(OblivionDetails set) {
            foreach (var collection in set.Collections) await RetrieveCollection(set.ChainID, collection.ID, true);

            for (var id = set.Collections.Count; id < set.TotalCollections; id++) 
                await RetrieveCollection(set.ChainID, Convert.ToUInt32(id), true);
        }

        private async Task UpdateReleases(OblivionDetails set) {
            foreach (var release in set.Releases.Where(a => !a.Ended)) 
                await RetrieveRelease(set.ChainID, release.ID, true);

            for (var id = set.Releases.Count; id < set.TotalReleases; id++) 
                await RetrieveRelease(set.ChainID, Convert.ToUInt32(id), true);
        }

        private async Task FinalizeListing(ChainID chainID, int version, ListingDetails listing) {
            listing.Finalized = true;
            var sale = await _blockchain.CheckSale(chainID, version, listing);
            if (sale != null) {
                sale.Nft = listing.NFT;
                listing.WasSold = true;
                listing.SaleInformation = sale;
            }
        }

        private static async Task UpdateSaleCollections(OblivionDetails set) {
            await Task.Run(() => {
                var sales = set.Listings.Where(a => a.WasSold);
                foreach (var sale in sales) {
                    var collection = set.Collections.Find(a => a.Nfts.Contains(sale.NFT));
                    if (collection != null) sale.SaleInformation.CollectionId = collection.ID;
                    else sale.SaleInformation.CollectionId = null;
                }
            });
        }

        private async Task UpdateTokens(ChainID chainID) {
            var payments = Globals.Payments.Find(a => a.ChainID == chainID);
            if (payments == null) return;

            foreach (var token in payments.PaymentTokens) token.Price = await _lookup.GetCurrentPrice(token.CoinGeckoKey);
        }

        private async Task UpdateReleaseSales(OblivionDetails set) {
            if (set.ReleaseStartingBlock == 0) return;
            if (set.LastReleaseScannedBlock == 0) set.LastReleaseScannedBlock = set.ReleaseStartingBlock - 1;

            var lastBlock = await _blockchain.GetLatestBlock(set.ChainID);
            var blocksToScan = lastBlock - set.LastReleaseScannedBlock;

            while (blocksToScan > 0) {
                var start = set.LastReleaseScannedBlock;
                uint end;

                if (blocksToScan > 5000) {
                    end = start + 5000;
                    blocksToScan -= 5000;
                }
                else {
                    end = start + blocksToScan;
                    blocksToScan = 0;
                }

                var sales = await _blockchain.CheckReleaseSales(set.ChainID, start, end);
                set.LastReleaseScannedBlock = end;
                
                if (sales == null) return;
                if (sales.Count == 0) continue;

                foreach (var sale in sales) {
                    var release = set.Releases.Find(a => a.ID == sale.ID);
                    sale.Price = release?.Price;
                    sale.PaymentToken = release?.PaymentToken;
                    set.ReleaseSales.Add(sale);
                }
            }
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
        
        private async Task<NFTDetails> RetrieveNFT(ChainID chainID, string address, bool forceUpdate) {
            var details = _details.Find(a => a.ChainID == chainID);
            if (details == null) return null;

            var nft = details.NFTs.Find(a => a.Address == address);
            if (nft == null || forceUpdate) {
                nft = await _blockchain.GetNFTDetails(chainID, address);
                if (nft != null) details.NFTs.Add(nft);
            }

            if (nft == null) return null;

            if (nft is { Metadata: null } || forceUpdate) {
                var metadata = await _lookup.GetNFTMetadata(nft.URI);
                if (metadata != null || forceUpdate) {
                    nft.Metadata = new NFTMetadata(metadata);
                    var cache = await _imageCache.ImageCache(chainID, address, nft.Metadata.Image, forceUpdate);
                    nft.CacheHighRes = !string.IsNullOrEmpty(cache.HighResImage) ? cache.HighResImage : nft.Metadata.Image;
                    if (!string.IsNullOrEmpty(cache.LowResImage)) nft.CacheLowRes = cache.LowResImage;
                    else nft.CacheLowRes = !string.IsNullOrEmpty(cache.HighResImage) ? cache.HighResImage : nft.Metadata.Image;
                }
            }


            return nft;
        }
        
        private async Task<NFTTokenIDInfo> RetrieveNFTTokenURI(ChainID chainID, string address, uint tokenID) {
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

            if (token is { Metadata: null }) {
                var metadata = await _lookup.GetNFTMetadata(token.URI);
                if (metadata != null) token.Metadata = new NFTMetadata(metadata);
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
    }
}
