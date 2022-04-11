/*
 *  OblivionAPI :: DatabaseService
 *
 *  This service is used to store and manage the in-memory database.
 * 
 */

using Microsoft.Extensions.Logging;
using OblivionAPI.Config;
using OblivionAPI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OblivionAPI.Services {
    public class DatabaseService {
        private readonly ILogger<DatabaseService> _logger;
        private readonly BlockchainService _blockchain;
        private readonly LookupService _lookup;

        private readonly List<OblivionDetails> _details;

        public DatabaseService(ILogger<DatabaseService> logger, BlockchainService blockchain, LookupService lookup) {
            _logger = logger;
            _blockchain = blockchain;
            _lookup = lookup;
            _details = new List<OblivionDetails> {
                new() { ChainID = ChainID.BSC_Mainnet },
                new() { ChainID = ChainID.BSC_Testnet }
            };
        }

        public async Task<List<OblivionSaleInformation>> GetSales(ChainID chainID) {
            var details = _details.Find(a => a.ChainID == chainID);
            var listings = details?.Listings.Where(a => a.WasSold);
            return listings?.Select(listing => listing.SaleInformation).ToList();
        }

        public async Task<List<ListingDetails>> GetListings(ChainID chainID) {
            var details = _details.Find(a => a.ChainID == chainID);
            return details?.Listings.ToList();
        }

        public async Task<List<OfferDetails>> GetOffers(ChainID chainID, uint id) {
            var details = _details.Find(a => a.ChainID == chainID);
            if (details == null) return null;
            var listing = details.Listings.Find(a => a.ID == id);
            return listing?.Offers.ToList();
        }

        public async Task<ListingDetails> ListingDetails(ChainID chainID, uint id) {
            var details = _details.Find(a => a.ChainID == chainID);
            if (details == null) return null;
            
            var listing = details.Listings.Find(a => a.ID == id);
            
            if (listing == null) {
                var result = await _blockchain.GetListing(chainID, id);
                if (result == null) return null;
                listing = result;
                details.Listings.Add(listing);
            }

            if (DateTime.Now - listing.LastRetrieved > TimeSpan.FromMinutes(Globals.CACHE_TIME)) {
                var result = await _blockchain.GetListing(chainID, id);
                listing.Update(result);
            }
            
            return listing;
        }

        public async Task<OfferDetails> OfferDetails(ChainID chainID, uint id, string paymentToken, uint offerID) {
            var details = _details.Find(a => a.ChainID == chainID);
            if (details == null) return null;
            
            var listing = details.Listings.Find(a => a.ID == id);
            if (listing == null) return null;

            var offer = listing.Offers.Find(a => a.PaymentToken == paymentToken && a.ID == offerID);
            if (offer == null) {
                var result = await _blockchain.GetOffer(chainID, id, paymentToken, offerID);
                if (result == null) return null;
                offer = result;
                listing.Offers.Add(result);
            }

            if (DateTime.Now - offer.LastRetrieved > TimeSpan.FromMinutes(Globals.CACHE_TIME)) {
                var result = await _blockchain.GetOffer(chainID, id, paymentToken, offerID);
                offer.Update(result);
            }

            return offer;
        }

        public async Task<List<OblivionDetails>> GetDetails() {
            return _details.ToList();
        }

        public async Task UpdateDetails(OblivionDetails details) {
            _logger.LogInformation("Updating details for chain {ChainID}", details.ChainID);
            var check = _details.Find(a => a.ChainID == details.ChainID);
            if (check == null) return;
            _details.Remove(check);
            _details.Add(check);
        }

        public async Task<uint> GetTotalListings(ChainID chainID) {
            var details = _details.Find(a => a.ChainID == chainID);
            if (details == null) return 0;
            
            if (DateTime.Now - details.LastRetrieved > TimeSpan.FromMinutes(Globals.CACHE_TIME)) await UpdateBasicDetails(details);
            return details.TotalListings;
        }

        public async Task<uint> GetTotalOffers(ChainID chainID, uint id) {
            var details = _details.Find(a => a.ChainID == chainID);
            if (details == null) return 0;

            var listing = details.Listings.Find(a => a.ID == id);
            if (listing == null) return 0;

            return Convert.ToUInt32(listing.Offers.Count);
        }

        public async Task<uint> GetTotalCollections(ChainID chainID) {
            var details = _details.Find(a => a.ChainID == chainID);
            if (details == null) return 0;

            if (DateTime.Now - details.LastRetrieved > TimeSpan.FromMinutes(Globals.CACHE_TIME)) await UpdateBasicDetails(details);
            return details.TotalCollections;
        }

        public async Task<List<CollectionDetails>> GetCollections(ChainID chainID) {
            var details = _details.Find(a => a.ChainID == chainID);
            return details?.Collections.ToList();
        }

        public async Task<CollectionDetails> GetCollection(ChainID chainID, uint id) {
            var details = _details.Find(a => a.ChainID == chainID);
            if (details == null) return null;

            var collection = details.Collections.Find(a => a.ID == id);
            if (collection == null) {
                var check = await _blockchain.GetCollection(chainID, id);
                if (check == null) return null;
                details.Collections.Add(check);
                collection = check;
            }

            if (DateTime.Now - collection.LastRetrieved > TimeSpan.FromMinutes(Globals.CACHE_TIME)) {
                var update = await _blockchain.GetCollection(chainID, id);
                if (update != null) collection.Update(update);
            }

            return collection;
        }

        private async Task UpdateBasicDetails(OblivionDetails details) {
            details.TotalListings = await _blockchain.GetTotalListings(details.ChainID);
            details.TotalCollections = await _blockchain.GetTotalCollections(details.ChainID);
            details.TotalReleases = await _blockchain.GetTotalReleases(details.ChainID);
            details.LastRetrieved = DateTime.Now;
        }

        public async Task<NFTDetails> GetNFT(ChainID chainID, string address) {
            var details = _details.Find(a => a.ChainID == chainID);
            if (details == null) return null;

            var nft = details.NFTs.Find(a => a.Address == address);
            if (nft == null) {
                nft = await _blockchain.GetNFTDetails(chainID, address);
                if (nft != null) {
                    details.NFTs.Add(nft);
                }
            }

            if (nft != null && nft.Metadata == null) {
                var metadata = await _lookup.GetNFTMetadata(nft.URI);
                if (metadata != null) nft.Metadata = new NFTMetadata(metadata);
            }
            
            return nft;
        }

        public async Task<NFTTokenIDInfo> GetNFTTokenURI(ChainID chainID, string address, uint tokenID) {
            var details = _details.Find(a => a.ChainID == chainID);
            if (details == null) return null;

            var nft = details.NFTs.Find(a => a.Address == address);
            
            if (nft == null) {
                nft = await _blockchain.GetNFTDetails(chainID, address);
                if (nft != null) {
                    var metadata = await _lookup.GetNFTMetadata(nft.URI);
                    if (metadata != null) nft.Metadata = new NFTMetadata(metadata);
                    details.NFTs.Add(nft);
                }
                else return null;
            }

            var token = nft.TokenDetails.Find(a => a.TokenId == tokenID);
            if (token == null) {
                var uri = await _blockchain.GetNFTTokenURI(chainID, address, tokenID);
                if (uri != null) {
                    token = new NFTTokenIDInfo { TokenId = tokenID, URI = uri };
                    nft.TokenDetails.Add(token);
                }
            }

            if (token != null && token.Metadata == null) {
                var metadata = await _lookup.GetNFTMetadata(token.URI);
                if (metadata != null) token.Metadata = new NFTMetadata(metadata);
            }

            return token;
        }
        
        public async Task<uint> GetTotalReleases(ChainID chainID) {
            var details = _details.Find(a => a.ChainID == chainID);
            if (details == null) return 0;

            if (DateTime.Now - details.LastRetrieved > TimeSpan.FromMinutes(Globals.CACHE_TIME)) await UpdateBasicDetails(details);
            return details.TotalReleases;
        }
        
        public async Task<List<ReleaseDetails>> GetReleases(ChainID chainID) {
            var details = _details.Find(a => a.ChainID == chainID);
            return details?.Releases.ToList();
        }
        
        public async Task<ReleaseDetails> GetRelease(ChainID chainID, uint id) {
            var details = _details.Find(a => a.ChainID == chainID);
            if (details == null) return null;

            var release = details.Releases.Find(a => a.ID == id);
            if (release == null) {
                var check = await _blockchain.GetRelease(chainID, id);
                if (check == null) return null;
                details.Releases.Add(check);
                release = check;
            }

            if (DateTime.Now - release.LastRetrieved > TimeSpan.FromMinutes(Globals.CACHE_TIME)) {
                var update = await _blockchain.GetRelease(chainID, id);
                if (update != null) release.Update(update);
            }

            return release;
        }

        public async Task<List<PaymentTokenDetails>> GetPaymentTokens(ChainID chainID) {
            var tokens = Globals.Payments.Find(a => a.ChainID == chainID);
            return tokens?.PaymentTokens;
        }
        
        public async Task HandleUpdate() {
            var details = await GetDetails();

            foreach (var set in details) {
                set.TotalListings = await _blockchain.GetTotalListings(set.ChainID);
                set.TotalCollections = await _blockchain.GetTotalCollections(set.ChainID);
                set.TotalReleases = await _blockchain.GetTotalReleases(set.ChainID);

                await UpdateListings(set);
                await UpdateCollections(set);
                await UpdateReleases(set);
                await UpdateTokens(set.ChainID);
                
                set.LastRetrieved = DateTime.Now;
                await UpdateDetails(set);
            }
        }

        private async Task UpdateListings(OblivionDetails set) {
            foreach (var listing in set.Listings.Where(a => !a.Finalized)) {
                var check = await _blockchain.GetListing(set.ChainID, listing.ID);
                if (check == null) continue;
                listing.Update(check);
            }

            for (var id = set.Listings.Count; id < set.TotalListings; id++) {
                var newListing = await _blockchain.GetListing(set.ChainID, Convert.ToUInt32(id));
                if (newListing == null) continue;
                set.Listings.Add(newListing);
            }

            foreach (var listing in set.Listings.Where(a => !a.Finalized)) {
                var checkNft = set.NFTs.Find(a => a.Address == listing.NFT);
                if (checkNft == null) {
                    var nft = await _blockchain.GetNFTDetails(set.ChainID, listing.NFT);
                    if (nft != null) {
                        var metadata = await _lookup.GetNFTMetadata(nft.URI);
                        if (metadata != null) nft.Metadata = new NFTMetadata(metadata);
                        set.NFTs.Add(nft);
                    }
                }
                
                var payments = Globals.Payments.Find(a => a.ChainID == set.ChainID);
                if (payments == null) break;
                foreach (var token in payments.PaymentTokens) {
                    foreach (var offer in listing.Offers.Where(a => a.PaymentToken == token.Address && !a.Claimed)) {
                        var check = await _blockchain.GetOffer(set.ChainID, listing.ID, token.Address, offer.ID);
                        if (check == null) continue;
                        offer.Update(check);
                    }
                    
                    var total = await _blockchain.GetListingOffers(set.ChainID, listing.ID, token.Address);
                    for (var id = listing.Offers.Count; id < total; id++) {
                        var newOffer = await _blockchain.GetOffer(set.ChainID, listing.ID, token.Address, Convert.ToUInt32(id));
                        if (newOffer == null) continue;
                        listing.Offers.Add(newOffer);
                    }
                }
            }
            
            foreach (var listing in set.Listings.Where(a => !a.Finalized && a.SaleState != 0)) await FinalizeListing(set.ChainID, listing);
        }

        private async Task UpdateCollections(OblivionDetails set) {
            foreach (var collection in set.Collections) {
                var details = await _blockchain.GetCollection(set.ChainID, collection.ID);
                if (details == null) continue;
                collection.Update(details);
            }

            for (var id = set.Collections.Count; id < set.TotalCollections; id++) {
                var details = await _blockchain.GetCollection(set.ChainID, Convert.ToUInt32(id));
                if (details != null) set.Collections.Add(details);
            }
        }

        private async Task UpdateReleases(OblivionDetails set) {
            foreach (var release in set.Releases.Where(a => !a.Ended)) {
                var details = await _blockchain.GetRelease(set.ChainID, release.ID);
                if (details == null) continue;
                release.Update(details);
            }

            for (var id = set.Releases.Count; id < set.TotalReleases; id++) {
                var details = await _blockchain.GetRelease(set.ChainID, Convert.ToUInt32(id));
                if (details != null) set.Releases.Add(details);
            }
        }

        private async Task FinalizeListing(ChainID chainID, ListingDetails listing) {
            listing.Finalized = true;
            var sale = await _blockchain.CheckSale(chainID, listing);
            if (sale != null) {
                listing.WasSold = true;
                listing.SaleInformation = sale;
            }
        }

        private async Task UpdateTokens(ChainID chainID) {
            var payments = Globals.Payments.Find(a => a.ChainID == chainID);
            if (payments == null) return;

            foreach (var token in payments.PaymentTokens) token.Price = await _lookup.GetCurrentPrice(token.CoinGeckoKey);
        }
    }
}
