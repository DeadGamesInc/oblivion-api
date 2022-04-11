﻿/*
 *  OblivionAPI :: BlockchainService
 *
 *  This service is used to read data from smart contracts.
 * 
 */

using Microsoft.Extensions.Logging;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using OblivionAPI.Objects;
using OblivionAPI.Config;
using OblivionAPI.Responses;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OblivionAPI.Services {
    public class BlockchainService {
        private readonly ILogger<BlockchainService> _logger;
        private readonly Web3 _bsc;
        private readonly Web3 _bscTestnet;

        public BlockchainService(ILogger<BlockchainService> logger) {
            _logger = logger;
            var bsc = Globals.Blockchains.Find(a => a.ChainID == ChainID.BSC_Mainnet);
            if (bsc != null) _bsc = new Web3(bsc.Node) { TransactionManager = { UseLegacyAsDefault = true } };
            var bscTestnet = Globals.Blockchains.Find(a => a.ChainID == ChainID.BSC_Testnet);
            if (bscTestnet != null) _bscTestnet = new Web3(bscTestnet.Node) { TransactionManager = { UseLegacyAsDefault = true } };
        }

        public async Task<NFTDetails> GetNFTDetails(ChainID chainID, string address) {
            try {
                _logger.LogDebug("Retrieving details for NFT {Address} on {ChainID}", address, chainID);

                var web3 = chainID switch {
                    ChainID.BSC_Mainnet => _bsc,
                    ChainID.BSC_Testnet => _bscTestnet,
                    _ => null
                };

                if (web3 == null) return null;

                var nft = new NFTDetails { Address = address };

                var contract = web3.Eth.GetContract(ABIs.OblivionNFT, address);

                var getName = contract.GetFunction("name");
                nft.Name = await getName.CallAsync<string>();

                var getSymbol = contract.GetFunction("symbol");
                nft.Symbol = await getSymbol.CallAsync<string>();

                var getURI = contract.GetFunction("tokenURI");
                nft.URI = await getURI.CallAsync<string>(1);

                return nft;
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured while getting NFT details for {Address} on {ChainID}", address, chainID);
                return null;
            }
        }

        public async Task<string> GetNFTTokenURI(ChainID chainID, string address, uint tokenID) {
            try {
                _logger.LogDebug("Retrieving details for tokenID {TokenID} on NFT {Address} on {ChainID}", tokenID, address,
                    chainID);

                var web3 = chainID switch {
                    ChainID.BSC_Mainnet => _bsc,
                    ChainID.BSC_Testnet => _bscTestnet,
                    _ => null
                };

                if (web3 == null) return null;

                var contract = web3.Eth.GetContract(ABIs.OblivionNFT, address);
                var getURI = contract.GetFunction("tokenURI");
                var uri = await getURI.CallAsync<string>(tokenID);

                return uri;
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured while getting NFT token details on {ChainID}", chainID);
                return null;
            }
        }

        public async Task<uint> GetTotalListings(ChainID chainID) {
            try {
                _logger.LogDebug("Retrieving total listings for {ChainID}", chainID);

                var address = Contracts.OblivionMarket.GetAddress(chainID);
                var web3 = chainID switch {
                    ChainID.BSC_Mainnet => _bsc,
                    ChainID.BSC_Testnet => _bscTestnet,
                    _ => null
                };

                if (web3 == null || string.IsNullOrEmpty(address)) return 0;

                var contract = web3.Eth.GetContract(ABIs.OblivionMarket, address);
                var getFunction = contract.GetFunction("totalListings");
                var result = await getFunction.CallAsync<uint>();

                return result;
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured while getting the total listings on {ChainID}", chainID);
                return 0;
            }
        }

        public async Task<uint> GetListingOffers(ChainID chainID, uint id, string paymentToken) {
            try {
                _logger.LogDebug("Retrieving offer count for listing {ID} with payment token {PaymentToken} on {ChainID}", id,
                    paymentToken, chainID);

                var address = Contracts.OblivionMarket.GetAddress(chainID);
                var web3 = chainID switch {
                    ChainID.BSC_Mainnet => _bsc,
                    ChainID.BSC_Testnet => _bscTestnet,
                    _ => null
                };

                if (web3 == null || string.IsNullOrEmpty(address)) return 0;

                var contract = web3.Eth.GetContract(ABIs.OblivionMarket, address);
                var getFunction = contract.GetFunction("totalOffers");
                var result = await getFunction.CallAsync<uint>(id, paymentToken);

                return result;
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured while getting offer count for {ListingID} on {ChainID}", id, chainID);
                return 0;
            }
        }

        public async Task<ListingDetails> GetListing(ChainID chainID, uint id) {
            try {
                _logger.LogDebug("Retrieving listing details for {ID} on {ChainID}", id, chainID);

                var address = Contracts.OblivionMarket.GetAddress(chainID);
                var web3 = chainID switch {
                    ChainID.BSC_Mainnet => _bsc,
                    ChainID.BSC_Testnet => _bscTestnet,
                    _ => null
                };

                if (web3 == null || string.IsNullOrEmpty(address)) return null;

                var contract = web3.Eth.GetContract(ABIs.OblivionMarket, address);
                var getFunction = contract.GetFunction("listings");
                var result = await getFunction.CallAsync<ListingResponse>(id);
                
                return new ListingDetails(id, result);
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured retrieving details for listing id {ID} on {ChainID}", id, chainID);
                return null;
            }
        }

        public async Task<OfferDetails> GetOffer(ChainID chainID, uint listingID, string paymentToken, uint offerID) {
            try {
                _logger.LogDebug("Retrieving details for offer {PaymentToken}:{OfferID} on listing {ListingID} on {ChainID}", paymentToken, offerID, listingID, chainID);
                var address = Contracts.OblivionMarket.GetAddress(chainID);
                var web3 = chainID switch {
                    ChainID.BSC_Mainnet => _bsc,
                    ChainID.BSC_Testnet => _bscTestnet,
                    _ => null
                };

                if (web3 == null || string.IsNullOrEmpty(address)) return null;

                var contract = web3.Eth.GetContract(ABIs.OblivionMarket, address);
                var getFunction = contract.GetFunction("offers");

                var result = await getFunction.CallAsync<OfferResponse>(listingID, paymentToken, offerID);
                
                return new OfferDetails(paymentToken, offerID, result);
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured retrieving details for offer {PaymentToken}:{OfferID} on listing {ListingID} on {ChainID}", paymentToken, offerID, listingID, chainID);
                return null;
            }
        }

        public async Task<OblivionSaleInformation> CheckSale(ChainID chainID, ListingDetails listing) {
            _logger.LogDebug("Checking sale details for {ListingID} on {ChainID}", listing.ID, chainID);
            
            try {
                Thread.Sleep(Globals.THROTTLE_WAIT);
                var address = Contracts.OblivionMarket.GetAddress(chainID);
                var web3 = chainID switch {
                    ChainID.BSC_Mainnet => _bsc,
                    ChainID.BSC_Testnet => _bscTestnet,
                    _ => null
                };

                if (web3 == null || string.IsNullOrEmpty(address)) return null;

                var contract = web3.Eth.GetContract(ABIs.OblivionMarket, address);

                var createBlock = new BlockParameter(Convert.ToUInt32(listing.CreateBlock));
                var block = new BlockParameter(Convert.ToUInt32(listing.ClosedBlock));
                
                var buyEvent = contract.GetEvent("DirectBuy");
                var buyFilter = buyEvent.CreateFilterInput(block, block);
                var buyEvents = await buyEvent.GetAllChangesDefaultAsync(buyFilter);

                if (buyEvents.Count > 0) {
                    var sale = new OblivionSaleInformation {
                        ID = listing.ID, Amount = buyEvents[0].Event[3].Result.ToString(), Buyer = buyEvents[0].Event[1].Result.ToString(),
                        Seller = listing.Owner, PaymentToken = buyEvents[0].Event[2].Result.ToString(), CreateDate = await GetBlockTimestamp(chainID, createBlock), SaleDate = await GetBlockTimestamp(chainID, block)
                    };
                    return sale;
                }
                
                var offerEvent = contract.GetEvent("AcceptOffer");
                var offerFilter = offerEvent.CreateFilterInput(block, block);
                var offerEvents = await offerEvent.GetAllChangesDefaultAsync(offerFilter);

                if (offerEvents.Count > 0) {
                    var sale = new OblivionSaleInformation {
                        ID = listing.ID, Amount = offerEvents[0].Event[4].Result.ToString(), Buyer = offerEvents[0].Event[2].Result.ToString(),
                        Seller = listing.Owner, PaymentToken = offerEvents[0].Event[1].Result.ToString(), CreateDate = await GetBlockTimestamp(chainID, createBlock), SaleDate = await GetBlockTimestamp(chainID, block)
                    };
                    return sale;
                }
                
                return null;
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured while checking sales information for {ListingID} on {ChainID}", chainID, listing.ID);
                return null;
            }
        }

        private async Task<DateTime> GetBlockTimestamp(ChainID chainID, BlockParameter block) {
            try {
                var web3 = chainID switch
                {
                    ChainID.BSC_Mainnet => _bsc,
                    ChainID.BSC_Testnet => _bscTestnet,
                    _ => null
                };

                if (web3 == null) return DateTime.MinValue;

                var txBlock = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(block);
                return DateTimeOffset.FromUnixTimeSeconds((long)txBlock.Timestamp.Value).UtcDateTime;
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured while getting block timestamp on {ChainID}", chainID);
                return DateTime.MinValue;
            }
        }

        public async Task<uint> GetTotalCollections(ChainID chainID) {
            try {
                _logger.LogDebug("Getting total collections on {ChainID}", chainID);
                var address = Contracts.OblivionCollectionManager.GetAddress(chainID);

                var web3 = chainID switch {
                    ChainID.BSC_Mainnet => _bsc,
                    ChainID.BSC_Testnet => _bscTestnet,
                    _ => null
                };

                if (web3 == null || string.IsNullOrEmpty(address)) return 0;

                var contract = web3.Eth.GetContract(ABIs.OblivionCollectionManager, address);
                var getFunction = contract.GetFunction("totalCollections");
                return await getFunction.CallAsync<uint>();
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured while retrieving total collections on {ChainID}", chainID);
                return 0;
            }
        }

        public async Task<CollectionDetails> GetCollection(ChainID chainID, uint id) {
            try {
                _logger.LogDebug("Getting collection details for {ID} on {ChainID}", id, chainID);
                var address = Contracts.OblivionCollectionManager.GetAddress(chainID);

                var web3 = chainID switch {
                    ChainID.BSC_Mainnet => _bsc,
                    ChainID.BSC_Testnet => _bscTestnet,
                    _ => null
                };

                if (web3 == null || string.IsNullOrEmpty(address)) return null;

                var contract = web3.Eth.GetContract(ABIs.OblivionCollectionManager, address);
                var getFunction = contract.GetFunction("collections");
                var result = await getFunction.CallAsync<CollectionResponse>(id);

                var nftsFunction = contract.GetFunction("collectionNfts");
                var nfts = await nftsFunction.CallAsync<CollectionNFTsResponse>(id);
                
                var collection = new CollectionDetails(id, result, nfts.NFTs.ToArray());
                
                return collection;
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured while retrieving collection {ID} on {ChainID}", id, chainID);
                return null;
            }
        }
        
        public async Task<uint> GetTotalReleases(ChainID chainID) {
            try {
                _logger.LogDebug("Getting total releases on {ChainID}", chainID);
                var address = Contracts.OblivionMintingService.GetAddress(chainID);

                var web3 = chainID switch {
                    ChainID.BSC_Mainnet => _bsc,
                    ChainID.BSC_Testnet => _bscTestnet,
                    _ => null
                };

                if (web3 == null || string.IsNullOrEmpty(address)) return 0;

                var contract = web3.Eth.GetContract(ABIs.OblivionMintingService, address);
                var getFunction = contract.GetFunction("totalListings");
                return await getFunction.CallAsync<uint>();
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured while retrieving total releases on {ChainID}", chainID);
                return 0;
            }
        }
        
        public async Task<ReleaseDetails> GetRelease(ChainID chainID, uint id) {
            try {
                _logger.LogDebug("Retrieving release details for {ID} on {ChainID}", id, chainID);

                var address = Contracts.OblivionMintingService.GetAddress(chainID);
                var web3 = chainID switch {
                    ChainID.BSC_Mainnet => _bsc,
                    ChainID.BSC_Testnet => _bscTestnet,
                    _ => null
                };

                if (web3 == null || string.IsNullOrEmpty(address)) return null;

                var contract = web3.Eth.GetContract(ABIs.OblivionMintingService, address);
                var getFunction = contract.GetFunction("listings");
                var result = await getFunction.CallAsync<ReleaseResponse>(id);

                var getTreasury = contract.GetFunction("getTreasuryInfo");
                var treasury = await getTreasury.CallAsync<ReleaseTreasuryDetailsResponse>(id);
                
                return new ReleaseDetails(id, result, treasury);
            } catch (Exception error) {
                _logger.LogError(error, "An exception occured retrieving details for release id {ID} on {ChainID}", id, chainID);
                return null;
            }
        }
    }
}
