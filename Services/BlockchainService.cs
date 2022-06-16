/*
 *  OblivionAPI :: BlockchainService
 *
 *  This service is used to read data from smart contracts.
 * 
 */

using System.Text;

using Microsoft.Extensions.Logging;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Threading;

namespace OblivionAPI.Services; 

public class BlockchainService {
    private readonly List<BlockchainErrors> _errors;
    private readonly ILogger<BlockchainService> _logger;

    public BlockchainService(ILogger<BlockchainService> logger) {
        _logger = logger;
        _errors = new List<BlockchainErrors> { new BSCMainnetErrors(), new BSCTestnetErrors(), new NervosTestnetErrors() };
    }

    public async Task AddStatus(StringBuilder builder) {
        await Task.Run(() =>
        {
            builder.AppendLine("Web3 Errors");
            builder.AppendLine("===========");
            foreach (var chain in _errors) {
                builder.AppendLine($"Chain           : {chain.ChainID}");
                builder.AppendLine($"Timeouts        : {chain.Timeouts}");
                builder.AppendLine($"Contract Errors : {chain.ContractErrors}");
                builder.AppendLine($"Exceptions      : {chain.Exceptions}");
                builder.AppendLine("");
            }
        });
    }

    public async Task<NFTDetails> GetNFTDetails(ChainID chainID, string address) {
        var nft = new NFTDetails { Address = address };

        try {
            _logger.LogDebug("Retrieving details for NFT {Address} on {ChainID}", address, chainID);

            var web3 = GetWeb3(chainID);
            if (web3 == null) return null;

            var contract = web3.Eth.GetContract(ABIs.OblivionNFT, address);

            var getName = contract.GetFunction("name");
            nft.Name = await getName.CallAsync<string>();

            var getSymbol = contract.GetFunction("symbol");
            nft.Symbol = await getSymbol.CallAsync<string>();

            var getTotalSupply = contract.GetFunction("totalSupply");
            nft.TotalSupply = await getTotalSupply.CallAsync<uint>();

            try {
                var getBaseURI = contract.GetFunction("baseURI");
                nft.BaseURI = await getBaseURI.CallAsync<string>();
            }
            catch (Exception error) {
                _logger.LogDebug(error, "Failed to set baseURI");
                _errors.Find(a => a.ChainID == chainID)!.ContractErrors++;
            }

            var getURI = contract.GetFunction("tokenURI");
            nft.URI = await getURI.CallAsync<string>(1);

            return nft;
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            _logger.LogWarning("Web3 connection timed out getting NFT details on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Timeouts++;
            return nft;
        }
        catch (Nethereum.ABI.FunctionEncoding.SmartContractRevertException error) {
            if (error.Message.Contains("ERC721Metadata: URI query for nonexistent token")) 
                _logger.LogWarning("ERC721 nonexistent token revert on Token ID 0 for {Address} on {ChainID}", address, chainID);
            else 
                _logger.LogError(error, "A smart contract exception occurred on token ID 0 for {Address} on {ChainID}", address, chainID);
            
            _errors.Find(a => a.ChainID == chainID)!.ContractErrors++;
            return nft;
        }
        catch (Exception error) {
            _logger.LogError(error, "An exception occured while getting NFT details for {Address} on {ChainID}", address, chainID);
            _errors.Find(a => a.ChainID == chainID)!.Exceptions++;
            return nft;
        }
    }

    public async Task<string> GetNFTTokenURI(ChainID chainID, string address, uint tokenID) {
        try {
            _logger.LogDebug("Retrieving details for tokenID {TokenID} on NFT {Address} on {ChainID}", tokenID, address,
                chainID);

            var web3 = GetWeb3(chainID);
            if (web3 == null) return null;

            var contract = web3.Eth.GetContract(ABIs.OblivionNFT, address);
            var getURI = contract.GetFunction("tokenURI");
            var uri = await getURI.CallAsync<string>(tokenID);

            return uri;
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            _logger.LogWarning("Web3 connection timed out getting NFT token details on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Timeouts++;
            return null;
        }
        catch (Exception error) {
            _logger.LogError(error, "An exception occured while getting NFT token details on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Exceptions++;
            return null;
        }
    }

    public async Task<uint> GetTotalListings(ChainID chainID, int version) {
        try {
            _logger.LogDebug("Retrieving total listings for {ChainID}:V{Version}", chainID, version);

            var address = GetMarketAddress(chainID, version);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return 0;

            var contract = web3.Eth.GetContract(ABIs.OblivionMarket, address);
            var getFunction = contract.GetFunction("totalListings");
            var result = await getFunction.CallAsync<uint>();

            return result;
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            _logger.LogWarning("Web3 connection timed out getting total listings on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Timeouts++;
            return 0;
        }
        catch (Exception error) {
            _logger.LogError(error, "An exception occured while getting the total listings on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Exceptions++;
            return 0;
        }
    }

    public async Task<uint> GetListingOffers(ChainID chainID, int version, uint id, string paymentToken) {
        try {
            _logger.LogDebug("Retrieving offer count for listing {ID} with payment token {PaymentToken} on {ChainID}:V{Version}", id,
                paymentToken, chainID, version);

            var address = GetMarketAddress(chainID, version);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return 0;

            var contract = web3.Eth.GetContract(ABIs.OblivionMarket, address);
            var getFunction = contract.GetFunction("totalOffers");
            var result = await getFunction.CallAsync<uint>(id, paymentToken);

            return result;
        } 
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            _logger.LogWarning("Web3 connection timed out getting offer count on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Timeouts++;
            return 0;
        }
        catch (Exception error) {
            _logger.LogError(error, "An exception occured while getting offer count for {ListingID} on {ChainID}", id, chainID);
            _errors.Find(a => a.ChainID == chainID)!.Exceptions++;
            return 0;
        }
    }

    public async Task<ListingDetails> GetListing(ChainID chainID, int version, uint id) {
        try {
            _logger.LogDebug("Retrieving listing details for {ID} on {ChainID}:V{Version}", id, chainID, version);

            var address = GetMarketAddress(chainID, version);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return null;

            var contract = web3.Eth.GetContract(ABIs.OblivionMarket, address);
            var getFunction = contract.GetFunction("listings");
            var result = await getFunction.CallAsync<ListingResponse>(id);
                
            return new ListingDetails(id, version, result);
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            _logger.LogWarning("Web3 connection timed out getting listing on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Timeouts++;
            return null;
        }
        catch (Exception error) {
            _logger.LogError(error, "An exception occured retrieving details for listing id {ID} on {ChainID}", id, chainID);
            _errors.Find(a => a.ChainID == chainID)!.Exceptions++;
            return null;
        }
    }

    public async Task<OfferDetails> GetOffer(ChainID chainID, int version, uint listingID, string paymentToken, uint offerID) {
        try {
            _logger.LogDebug("Retrieving details for offer {PaymentToken}:{OfferID} on listing {ListingID} on {ChainID}:V{Version}", paymentToken, offerID, listingID, chainID, version);
            var address = GetMarketAddress(chainID, version);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return null;

            var contract = web3.Eth.GetContract(ABIs.OblivionMarket, address);
            var getFunction = contract.GetFunction("offers");

            var result = await getFunction.CallAsync<OfferResponse>(listingID, paymentToken, offerID);
                
            return new OfferDetails(paymentToken, listingID, offerID, version, result);
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            _logger.LogWarning("Web3 connection timed out getting offer details on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Timeouts++;
            return null;
        }
        catch (Exception error) {
            _logger.LogError(error, "An exception occured retrieving details for offer {PaymentToken}:{OfferID} on listing {ListingID} on {ChainID}", paymentToken, offerID, listingID, chainID);
            _errors.Find(a => a.ChainID == chainID)!.Exceptions++;
            return null;
        }
    }

    public async Task<List<ReleaseSaleDetails>> CheckReleaseSales(ChainID chainID, uint startBlock, uint endBlock) {
        _logger.LogDebug("Checking release sales from block {StartBlock} to block {EndBlock} on {ChainID}", startBlock, endBlock, chainID);
        try {
            Thread.Sleep(Globals.THROTTLE_WAIT);
            var sales = new List<ReleaseSaleDetails>();

            var address = Contracts.OblivionMintingService.GetAddress(chainID);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return null;
                
            var contract = web3.Eth.GetContract(ABIs.OblivionMintingService, address);
            var start = new BlockParameter(startBlock);
            var end = new BlockParameter(endBlock);

            var singleEvent = contract.GetEvent("NftPurchased");
            var singleFilter = singleEvent.CreateFilterInput(start, end);
            var singleEvents = await singleEvent.GetAllChangesDefaultAsync(singleFilter);

            if (singleEvents.Count > 0) {
                foreach (var sale in singleEvents) {
                    var block = new BlockParameter(sale.Log.BlockNumber);
                    var saleTime = await GetBlockTimestamp(chainID, block);
                    var details = new ReleaseSaleDetails {
                        ID = Convert.ToUInt32(sale.Event[0].Result.ToString()),
                        Quantity = 1,
                        SaleTime = saleTime
                    };
                    sales.Add(details);
                }
            }

            var multiEvent = contract.GetEvent("MultiNftPurchases");
            var multiFilter = multiEvent.CreateFilterInput(start, end);
            var multiEvents = await multiEvent.GetAllChangesDefaultAsync(multiFilter);

            if (multiEvents.Count > 0) {
                foreach (var sale in multiEvents) {
                    var block = new BlockParameter(sale.Log.BlockNumber);
                    var saleTime = await GetBlockTimestamp(chainID, block);
                    var details = new ReleaseSaleDetails {
                        ID = Convert.ToUInt32(sale.Event[0].Result.ToString()),
                        Quantity = Convert.ToInt32(sale.Event[2].Result.ToString()),
                        SaleTime = saleTime
                    };
                    sales.Add(details);
                }
            }

            return sales;
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            _logger.LogWarning("Web3 connection timed out getting release sales on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Timeouts++;
            return null;
        }
        catch (Exception error) {
            _logger.LogError(error, "An exception occured while checking release sales");
            _errors.Find(a => a.ChainID == chainID)!.Exceptions++;
            return null;
        }
    }

    public async Task<OblivionSaleInformation> CheckSale(ChainID chainID, int version, ListingDetails listing) {
        _logger.LogDebug("Checking sale details for {ListingID} on {ChainID}:V{Version}", listing.ID, chainID, version);
            
        try {
            Thread.Sleep(Globals.THROTTLE_WAIT);
            var address = GetMarketAddress(chainID, version);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return null;

            var contract = web3.Eth.GetContract(ABIs.OblivionMarket, address);

            var createBlock = new BlockParameter(Convert.ToUInt32(listing.CreateBlock));
            var block = new BlockParameter(Convert.ToUInt32(listing.ClosedBlock));
                
            var buyEvent = contract.GetEvent("DirectBuy");
            var buyFilter = buyEvent.CreateFilterInput(block, block);
            var buyEvents = await buyEvent.GetAllChangesDefaultAsync(buyFilter);

            if (buyEvents.Count > 0) {
                var sale = new OblivionSaleInformation {
                    ID = listing.ID, Version = version, Amount = buyEvents[0].Event[3].Result.ToString(), Buyer = buyEvents[0].Event[1].Result.ToString(),
                    Seller = listing.Owner, PaymentToken = buyEvents[0].Event[2].Result.ToString(), CreateDate = await GetBlockTimestamp(chainID, createBlock), SaleDate = await GetBlockTimestamp(chainID, block), TxHash = buyEvents[0].Log.TransactionHash
                };
                return sale;
            }
                
            var offerEvent = contract.GetEvent("AcceptOffer");
            var offerFilter = offerEvent.CreateFilterInput(block, block);
            var offerEvents = await offerEvent.GetAllChangesDefaultAsync(offerFilter);

            if (offerEvents.Count > 0) {
                var sale = new OblivionSaleInformation {
                    ID = listing.ID, Version = version, Amount = offerEvents[0].Event[4].Result.ToString(), Buyer = offerEvents[0].Event[2].Result.ToString(),
                    Seller = listing.Owner, PaymentToken = offerEvents[0].Event[1].Result.ToString(), CreateDate = await GetBlockTimestamp(chainID, createBlock), SaleDate = await GetBlockTimestamp(chainID, block), TxHash = offerEvents[0].Log.TransactionHash
                };
                return sale;
            }

            var cancelEvent = contract.GetEvent("CancelListing");
            var cancelFilter = cancelEvent.CreateFilterInput(block, block);
            var cancelEvents = await cancelEvent.GetAllChangesDefaultAsync(cancelFilter);

            if (cancelEvents.Count > 0) {
                var sale = new OblivionSaleInformation {
                    ID = listing.ID, Cancelled = true, TxHash = cancelEvents[0].Log.TransactionHash
                };
                return sale;
            }
                
            return null;
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            _logger.LogWarning("Web3 connection timed out getting sales information on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Timeouts++;
            return null;
        }
        catch (Exception error) {
            _logger.LogError(error, "An exception occured while checking sales information for {ListingID} on {ChainID}", chainID, listing.ID);
            _errors.Find(a => a.ChainID == chainID)!.Exceptions++;
            return null;
        }
    }

    private async Task<DateTime> GetBlockTimestamp(ChainID chainID, BlockParameter block) {
        try {
            var web3 = GetWeb3(chainID);

            if (web3 == null) return DateTime.MinValue;

            var txBlock = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(block);
            return DateTimeOffset.FromUnixTimeSeconds((long)txBlock.Timestamp.Value).UtcDateTime;
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            _logger.LogWarning("Web3 connection timed out getting block timestamp on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Timeouts++;
            return DateTime.MinValue;
        }
        catch (Exception error) {
            _logger.LogError(error, "An exception occured while getting block timestamp on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Exceptions++;
            return DateTime.MinValue;
        }
    }

    public async Task<uint> GetLatestBlock(ChainID chainID) {
        try {
            var web3 = GetWeb3(chainID);
            if (web3 == null) return 0;

            var block = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            return Convert.ToUInt32(block.ToString());
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            _logger.LogWarning("Web3 connection timed out getting latest block number on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Timeouts++;
            return 0;
        }
        catch (Exception error) {
            _logger.LogError(error, "An exception occured while retrieving the latest block number on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Exceptions++;
            return 0;
        }
    }

    public async Task<uint> GetTotalCollections(ChainID chainID) {
        try {
            _logger.LogDebug("Getting total collections on {ChainID}", chainID);
            var address = Contracts.OblivionCollectionManager.GetAddress(chainID);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return 0;

            var contract = web3.Eth.GetContract(ABIs.OblivionCollectionManager, address);
            var getFunction = contract.GetFunction("totalCollections");
            return await getFunction.CallAsync<uint>();
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            _logger.LogWarning("Web3 connection timed out getting total collections on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Timeouts++;
            return 0;
        }
        catch (Exception error) {
            _logger.LogError(error, "An exception occured while retrieving total collections on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Exceptions++;
            return 0;
        }
    }

    public async Task<CollectionDetails> GetCollection(ChainID chainID, uint id) {
        try {
            _logger.LogDebug("Getting collection details for {ID} on {ChainID}", id, chainID);
            var address = Contracts.OblivionCollectionManager.GetAddress(chainID);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return null;

            var contract = web3.Eth.GetContract(ABIs.OblivionCollectionManager, address);
            var getFunction = contract.GetFunction("collections");
            var result = await getFunction.CallAsync<CollectionResponse>(id);

            var nftsFunction = contract.GetFunction("collectionNfts");
            var nfts = await nftsFunction.CallAsync<CollectionNFTsResponse>(id);

            var metaDataFunction = contract.GetFunction("getMetadata");

            var name = await metaDataFunction.CallAsync<string>(id, "name");
            var image = await metaDataFunction.CallAsync<string>(id, "image");
            var description = await metaDataFunction.CallAsync<string>(id, "description");
            var banner = await metaDataFunction.CallAsync<string>(id, "banner");
                
            var collection = new CollectionDetails(id, result, name, image, description, banner, nfts.NFTs.ToArray());
                
            return collection;
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            _logger.LogWarning("Web3 connection timed out getting collection details on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Timeouts++;
            return null;
        }
        catch (Exception error) {
            _logger.LogError(error, "An exception occured while retrieving collection {ID} on {ChainID}", id, chainID);
            _errors.Find(a => a.ChainID == chainID)!.Exceptions++;
            return null;
        }
    }
        
    public async Task<uint> GetTotalReleases(ChainID chainID) {
        try {
            _logger.LogDebug("Getting total releases on {ChainID}", chainID);
            var address = Contracts.OblivionMintingService.GetAddress(chainID);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return 0;

            var contract = web3.Eth.GetContract(ABIs.OblivionMintingService, address);
            var getFunction = contract.GetFunction("totalListings");
            return await getFunction.CallAsync<uint>();
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            _logger.LogWarning("Web3 connection timed out getting total releases on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Timeouts++;
            return 0;
        }
        catch (Exception error) {
            _logger.LogError(error, "An exception occured while retrieving total releases on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Exceptions++;
            return 0;
        }
    }
        
    public async Task<ReleaseDetails> GetRelease(ChainID chainID, uint id) {
        try {
            _logger.LogDebug("Retrieving release details for {ID} on {ChainID}", id, chainID);

            var address = Contracts.OblivionMintingService.GetAddress(chainID);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return null;

            var contract = web3.Eth.GetContract(ABIs.OblivionMintingService, address);
            var getFunction = contract.GetFunction("listings");
            var result = await getFunction.CallAsync<ReleaseResponse>(id);

            var getTreasury = contract.GetFunction("getTreasuryInfo");
            var treasury = await getTreasury.CallAsync<ReleaseTreasuryDetailsResponse>(id);
                
            return new ReleaseDetails(id, result, treasury);
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            _logger.LogWarning("Web3 connection timed out getting release details on {ChainID}", chainID);
            _errors.Find(a => a.ChainID == chainID)!.Timeouts++;
            return null;
        }
        catch (Exception error) {
            _logger.LogError(error, "An exception occured retrieving details for release id {ID} on {ChainID}", id, chainID);
            _errors.Find(a => a.ChainID == chainID)!.Exceptions++;
            return null;
        }
    }

    private static string GetMarketAddress(ChainID chainID, int version) {
        return version switch {
            1 => Contracts.OblivionMarket.GetAddress(chainID),
            2 => Contracts.OblivionMarketV2.GetAddress(chainID),
            _ => null
        };
    }

    private Web3 GetWeb3(ChainID chainID) {
        switch (chainID) {
            case ChainID.BSC_Mainnet:
                var bsc = Globals.Blockchains.Find(a => a.ChainID == ChainID.BSC_Mainnet);
                return bsc != null ? new Web3(bsc.Node) { TransactionManager = { UseLegacyAsDefault = true } } : null;
            
            case ChainID.BSC_Testnet:
                var bscTestnet = Globals.Blockchains.Find(a => a.ChainID == ChainID.BSC_Testnet);
                return bscTestnet != null ? new Web3(bscTestnet.Node) { TransactionManager = { UseLegacyAsDefault = true } } : null;
            
            case ChainID.Nervos_Testnet:
                var nervosTestnet = Globals.Blockchains.Find(a => a.ChainID == ChainID.Nervos_Testnet);
                return nervosTestnet != null ? new Web3(nervosTestnet.Node) { TransactionManager = { UseLegacyAsDefault = true } } : null;

            default: return null;
        }
    }
}