/*
 *  OblivionAPI :: BlockchainService
 *
 *  This service is used to read data from smart contracts.
 * 
 */

using System.Diagnostics;
using System.Text;

using Microsoft.Extensions.Logging;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Threading;

namespace OblivionAPI.Services; 

public class BlockchainService {
    private readonly List<BlockchainErrors> _errors;
    private readonly List<BlockchainStats> _stats;
    private readonly ILogger<BlockchainService> _logger;

    public BlockchainService(ILogger<BlockchainService> logger) {
        _logger = logger;
        _errors = new List<BlockchainErrors> { new BSCMainnetErrors(), new BSCTestnetErrors(), new NervosTestnetErrors(), new NervosMainnetErrors() };
        _stats = new List<BlockchainStats> { new BSCMainnetStats(), new BSCTestnetStats(), new NervosTestnetStats(), new NervosMainnetStats() };
    }

    public async Task AddStatus(StringBuilder builder) {
        await Task.Run(() => {
            builder.AppendLine("Blockchain Service Stats (This Hour)");
            builder.AppendLine("====================================");
            
            foreach (var chain in _stats) {
                builder.AppendLine($"Chain                       : {chain.ChainID}");
                builder.AppendLine($"Operations                  : {chain.Operations}");
                builder.AppendLine($"Average Operation Time (ms) : {chain.AverageOperationTime}");
                builder.AppendLine("");
            }
            
            builder.AppendLine("Blockchain Service Errors");
            builder.AppendLine("=========================");
            foreach (var chain in _errors) {
                builder.AppendLine($"Chain           : {chain.ChainID}");
                builder.AppendLine($"Timeouts        : {chain.Timeouts} | {chain.PreviousTimeouts} | {chain.TotalTimeouts}");
                builder.AppendLine($"Contract Errors : {chain.ContractErrors} | {chain.PreviousContractErrors} | {chain.TotalContractErrors}");
                builder.AppendLine($"Exceptions      : {chain.Exceptions} | {chain.PreviousExceptions} | {chain.TotalExceptions}");
                builder.AppendLine("");
            }
        });
    }

    public void ResetCounters() {
        foreach (var chain in _errors) {
            chain.PreviousTimeouts = chain.Timeouts;
            chain.PreviousExceptions = chain.Exceptions;
            chain.PreviousContractErrors = chain.ContractErrors;
            chain.Timeouts = 0;
            chain.ContractErrors = 0;
            chain.Exceptions = 0;
        }

        foreach (var chain in _stats) {
            chain.Operations = 0;
            chain.TotalOperationTime = 0;
            chain.AverageOperationTime = 0;
        }
    }

    private void HandleTimeout(ChainID chainID, string message) {
        _logger.LogWarning("{Message} on {ChainID}", message, chainID);
        _errors.Find(a => a.ChainID == chainID)!.Timeouts++;
        _errors.Find(a => a.ChainID == chainID)!.TotalTimeouts++;
    }

    private void HandleContractError(ChainID chainID, Exception error, string message) {
        _logger.LogWarning(error, "{Message} on {ChainID}", message, chainID);
        _errors.Find(a => a.ChainID == chainID)!.ContractErrors++;
        _errors.Find(a => a.ChainID == chainID)!.TotalContractErrors++;
    }
    
    private void HandleContractError(ChainID chainID, string message) {
        _logger.LogWarning("{Message} on {ChainID}", message, chainID);
        _errors.Find(a => a.ChainID == chainID)!.ContractErrors++;
        _errors.Find(a => a.ChainID == chainID)!.TotalContractErrors++;
    }

    private void HandleException(ChainID chainID, Exception error, string message) {
        _logger.LogError(error, "{Message} on {ChainID}", message, chainID);
        _errors.Find(a => a.ChainID == chainID)!.Exceptions++;
        _errors.Find(a => a.ChainID == chainID)!.TotalExceptions++;
    }

    public async Task<NFTType> GetNFTType(ChainID chainID, string address) {
        var timer = Stopwatch.StartNew();
        
        try {
            _logger.LogDebug("Checking NFT type for {Address} on {ChainID}", address, chainID);
            var web3 = GetWeb3(chainID);
            if (web3 == null) return NFTType.UNKNOWN;

            byte[] interfaceID = { 0xd9, 0xb6, 0x7a, 0x26 };

            var contract = web3.Eth.GetContract(ABIs.OblivionNFT1155, address);
            var supportsInterface = contract.GetFunction("supportsInterface");
            var isERC1155 = await supportsInterface.CallAsync<bool>(interfaceID);

            return isERC1155 ? NFTType.ERC1155 : NFTType.ERC721;
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting NFT details");
            else 
                HandleException(chainID, error, $"An exception occured while getting NFT details for {address}");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting NFT details");
            else 
                HandleException(chainID, error, $"An exception occured while getting NFT details for {address}");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting NFT details");
        }
        catch (Nethereum.ABI.FunctionEncoding.SmartContractRevertException error) {
            if (error.Message.Contains("ERC721Metadata: URI query for nonexistent token"))
                HandleContractError(chainID, $"ERC721 nonexistent token revert on Token ID 0 for {address}");
            else
                HandleContractError(chainID, error, $"A smart contract exception occurred on token ID 0 for {address}");
        }
        catch (Exception error) {
            HandleException(chainID, error, $"An exception occured while getting NFT details for {address}");
        }
        
        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return NFTType.UNKNOWN;
    }

    public async Task<NFTDetails> GetNFTDetails(ChainID chainID, string address) {
        var timer = Stopwatch.StartNew();
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
                HandleContractError(chainID, error, "Failed to set baseURI");
            }

            var getURI = contract.GetFunction("tokenURI");
            nft.URI = await getURI.CallAsync<string>(1);
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting NFT details");
            else 
                HandleException(chainID, error, $"An exception occured while getting NFT details for {address}");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting NFT details");
            else 
                HandleException(chainID, error, $"An exception occured while getting NFT details for {address}");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting NFT details");
        }
        catch (Nethereum.ABI.FunctionEncoding.SmartContractRevertException error) {
            if (error.Message.Contains("ERC721Metadata: URI query for nonexistent token"))
                HandleContractError(chainID, $"ERC721 nonexistent token revert on Token ID 0 for {address}");
            else
                HandleContractError(chainID, error, $"A smart contract exception occurred on token ID 0 for {address}");
        }
        catch (Exception error) {
            HandleException(chainID, error, $"An exception occured while getting NFT details for {address}");
        }
        
        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return nft;
    }

    public async Task<string> GetNFTTokenURI(ChainID chainID, string address, uint tokenID) {
        var timer = Stopwatch.StartNew();
        string uri = null;
        
        try {
            _logger.LogDebug("Retrieving details for tokenID {TokenID} on NFT {Address} on {ChainID}", tokenID, address,
                chainID);

            var web3 = GetWeb3(chainID);
            if (web3 == null) return null;

            var contract = web3.Eth.GetContract(ABIs.OblivionNFT, address);
            var getURI = contract.GetFunction("tokenURI");
            uri = await getURI.CallAsync<string>(tokenID);
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting NFT token details");
            else 
                HandleException(chainID, error, $"An exception occured while getting NFT token details for {address}");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting NFT token details");
            else 
                HandleException(chainID, error, $"An exception occured while getting NFT token details for {address}");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting NFT token details");
        }
        catch (Exception error) {
            HandleException(chainID, error, $"An exception occured while getting NFT token details for {address}");
        }
        
        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return uri;
    }

    public async Task<uint> GetTotalListings(ChainID chainID, int version) {
        var timer = Stopwatch.StartNew();
        uint total = 0;
        
        try {
            _logger.LogDebug("Retrieving total listings for {ChainID}:V{Version}", chainID, version);

            var address = GetMarketAddress(chainID, version);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return 0;

            var contract = web3.Eth.GetContract(ABIs.OblivionMarket, address);
            var getFunction = contract.GetFunction("totalListings");
            total = await getFunction.CallAsync<uint>();
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting total listings");
            else 
                HandleException(chainID, error, "An exception occured while getting total listings");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting total listings");
            else 
                HandleException(chainID, error, "An exception occured while getting total listings");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting total listings");
        }
        catch (Exception error) {
            HandleException(chainID, error, "An exception occured while getting the total listings");
        }

        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return total;
    }

    public async Task<uint> GetListingOffers(ChainID chainID, int version, uint id, string paymentToken) {
        var timer = Stopwatch.StartNew();
        uint offers = 0;
        
        try {
            _logger.LogDebug("Retrieving offer count for listing {ID} with payment token {PaymentToken} on {ChainID}:V{Version}", id,
                paymentToken, chainID, version);

            var address = GetMarketAddress(chainID, version);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return 0;

            var contract = web3.Eth.GetContract(ABIs.OblivionMarket, address);
            var getFunction = contract.GetFunction("totalOffers");
            offers = await getFunction.CallAsync<uint>(id, paymentToken);
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting offer count");
            else 
                HandleException(chainID, error, "An exception occured while getting offer count");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting offer count");
            else
                HandleException(chainID, error, "An exception occured while getting offer count");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting offer count");
        }
        catch (Exception error) {
            HandleException(chainID, error, $"An exception occured while getting offer count for {id}");
        }

        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return offers;
    }

    public async Task<ListingDetails> GetListing(ChainID chainID, int version, uint id) {
        var timer = Stopwatch.StartNew();
        ListingDetails listing = null;
        
        try {
            _logger.LogDebug("Retrieving listing details for {ID} on {ChainID}:V{Version}", id, chainID, version);

            var address = GetMarketAddress(chainID, version);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return null;

            var contract = web3.Eth.GetContract(ABIs.OblivionMarket, address);
            var getFunction = contract.GetFunction("listings");
            var result = await getFunction.CallAsync<ListingResponse>(id);
            listing = new ListingDetails(id, version, result);
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting listing");
            else 
                HandleException(chainID, error, $"An exception occured retrieving details for listing id {id}");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting listing");
            else 
                HandleException(chainID, error, $"An exception occured retrieving details for listing id {id}");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting listing");
        }
        catch (Exception error) {
            HandleException(chainID, error, $"An exception occured retrieving details for listing id {id}");
        }

        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return listing;
    }

    public async Task<OfferDetails> GetOffer(ChainID chainID, int version, uint listingID, string paymentToken, uint offerID) {
        var timer = Stopwatch.StartNew();
        OfferDetails offer = null;
        
        try {
            _logger.LogDebug("Retrieving details for offer {PaymentToken}:{OfferID} on listing {ListingID} on {ChainID}:V{Version}", paymentToken, offerID, listingID, chainID, version);
            var address = GetMarketAddress(chainID, version);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return null;

            var contract = web3.Eth.GetContract(ABIs.OblivionMarket, address);
            var getFunction = contract.GetFunction("offers");

            var result = await getFunction.CallAsync<OfferResponse>(listingID, paymentToken, offerID);
            offer = new OfferDetails(paymentToken, listingID, offerID, version, result);
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting offer details");
            else 
                HandleException(chainID, error, "An exception occured while getting offer details");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting offer details");
            else 
                HandleException(chainID, error, "An exception occured while getting offer details");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting offer details");
        }
        catch (Exception error) {
            HandleException(chainID, error, $"An exception occured retrieving details for offer {paymentToken}:{offerID} on listing {listingID}");
        }

        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return offer;
    }

    public async Task<List<ReleaseSaleDetails>> CheckReleaseSales(ChainID chainID, uint startBlock, uint endBlock) {
        var timer = Stopwatch.StartNew();
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
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting release sales");
            else 
                HandleException(chainID, error, "An exception occured while getting release sales");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting release sales");
            else 
                HandleException(chainID, error, "An exception occured while getting release sales");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting release sales");
        }
        catch (Exception error) {
            HandleException(chainID, error, "An exception occured while getting release sales");
        }
        
        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return null;
    }

    public async Task<OblivionSaleInformation> CheckSale(ChainID chainID, int version, ListingDetails listing) {
        var timer = Stopwatch.StartNew();
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

            OblivionSaleInformation sale;
            
            if (buyEvents.Count > 0) {
                sale = new OblivionSaleInformation {
                    ID = listing.ID, Version = version, Amount = buyEvents[0].Event[3].Result.ToString(), Buyer = buyEvents[0].Event[1].Result.ToString(),
                    Seller = listing.Owner, PaymentToken = buyEvents[0].Event[2].Result.ToString(), CreateDate = await GetBlockTimestamp(chainID, createBlock), SaleDate = await GetBlockTimestamp(chainID, block), TxHash = buyEvents[0].Log.TransactionHash
                };
                return sale;
            }
                
            var offerEvent = contract.GetEvent("AcceptOffer");
            var offerFilter = offerEvent.CreateFilterInput(block, block);
            var offerEvents = await offerEvent.GetAllChangesDefaultAsync(offerFilter);

            if (offerEvents.Count > 0) {
                sale = new OblivionSaleInformation {
                    ID = listing.ID, Version = version, Amount = offerEvents[0].Event[4].Result.ToString(), Buyer = offerEvents[0].Event[2].Result.ToString(),
                    Seller = listing.Owner, PaymentToken = offerEvents[0].Event[1].Result.ToString(), CreateDate = await GetBlockTimestamp(chainID, createBlock), SaleDate = await GetBlockTimestamp(chainID, block), TxHash = offerEvents[0].Log.TransactionHash
                };
                return sale;
            }

            var cancelEvent = contract.GetEvent("CancelListing");
            var cancelFilter = cancelEvent.CreateFilterInput(block, block);
            var cancelEvents = await cancelEvent.GetAllChangesDefaultAsync(cancelFilter);

            if (cancelEvents.Count > 0) {
                sale = new OblivionSaleInformation {
                    ID = listing.ID, Cancelled = true, TxHash = cancelEvents[0].Log.TransactionHash
                };
                return sale;
            }
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting sales information");
            else 
                HandleException(chainID, error, "An exception occured while getting sales information");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting sales information");
            else 
                HandleException(chainID, error, "An exception occured while getting sales information");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting sales information");
        }
        catch (Exception error) {
            HandleException(chainID, error, "An exception occured while getting sales information");
        }

        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return null;
    }

    private async Task<DateTime> GetBlockTimestamp(ChainID chainID, BlockParameter block) {
        var timer = Stopwatch.StartNew();
        
        try {
            var web3 = GetWeb3(chainID);

            if (web3 == null) return DateTime.MinValue;

            var txBlock = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(block);
            return DateTimeOffset.FromUnixTimeSeconds((long)txBlock.Timestamp.Value).UtcDateTime;
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting block timestamp");
            else 
                HandleException(chainID, error, "An exception occured while getting block timestamp");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting block timestamp");
            else 
                HandleException(chainID, error, "An exception occured while getting block timestamp");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting block timestamp");
        }
        catch (Exception error) {
            HandleException(chainID, error, "An exception occured while getting block timestamp");
        }
        
        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return DateTime.MinValue;
    }

    public async Task<uint> GetLatestBlock(ChainID chainID) {
        var timer = Stopwatch.StartNew();
        uint blockNumber = 0;
        
        try {
            var web3 = GetWeb3(chainID);
            if (web3 == null) return 0;

            var block = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            blockNumber = Convert.ToUInt32(block.ToString());
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting latest block number");
            else 
                HandleException(chainID, error, "An exception occured while getting latest block number");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting latest block number");
            else 
                HandleException(chainID, error, "An exception occured while getting latest block number");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting latest block number");
        }
        catch (Exception error) {
            HandleException(chainID, error, "An exception occured while getting latest block number");
        }

        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return blockNumber;
    }

    public async Task<uint> GetTotalCollections(ChainID chainID) {
        var timer = Stopwatch.StartNew();
        uint total = 0;
        
        try {
            _logger.LogDebug("Getting total collections on {ChainID}", chainID);
            var address = Contracts.OblivionCollectionManager.GetAddress(chainID);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return 0;

            var contract = web3.Eth.GetContract(ABIs.OblivionCollectionManager, address);
            var getFunction = contract.GetFunction("totalCollections");
            total = await getFunction.CallAsync<uint>();
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting total collections");
            else 
                HandleException(chainID, error, "An exception occured while getting total collections");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting total collections");
            else 
                HandleException(chainID, error, "An exception occured while getting total collections");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting total collections");
        }
        catch (Exception error) {
            HandleException(chainID, error, "An exception occured while getting total collections");
        }

        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return total;
    }

    public async Task<CollectionDetails> GetCollection(ChainID chainID, uint id) {
        var timer = Stopwatch.StartNew();
        CollectionDetails details = null;
        
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
                
            details = new CollectionDetails(id, result, name, image, description, banner, nfts.NFTs.ToArray());
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting collection details");
            else 
                HandleException(chainID, error, "An exception occured while getting collection details");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting collection details");
            else 
                HandleException(chainID, error, "An exception occured while getting collection details");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting collection details");
        }
        catch (Exception error) {
            HandleException(chainID, error, "An exception occured while getting collection details");
        }

        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return details;
    }
        
    public async Task<uint> GetTotalReleases(ChainID chainID) {
        var timer = Stopwatch.StartNew();
        uint total = 0;
        
        try {
            _logger.LogDebug("Getting total releases on {ChainID}", chainID);
            var address = Contracts.OblivionMintingService.GetAddress(chainID);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(address)) return 0;

            var contract = web3.Eth.GetContract(ABIs.OblivionMintingService, address);
            var getFunction = contract.GetFunction("totalListings");
            total = await getFunction.CallAsync<uint>();
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting total releases");
            else 
                HandleException(chainID, error, "An exception occured while getting total releases");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting total releases");
            else 
                HandleException(chainID, error, "An exception occured while getting total releases");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting total releases");
        }
        catch (Exception error) {
            HandleException(chainID, error, "An exception occured while getting total releases");
        }

        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return total;
    }
        
    public async Task<ReleaseDetails> GetRelease(ChainID chainID, uint id) {
        var timer = Stopwatch.StartNew();
        ReleaseDetails details = null;
        
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
                
            details = new ReleaseDetails(id, result, treasury);
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting release details");
            else 
                HandleException(chainID, error, "An exception occured while getting release details");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting release details");
            else 
                HandleException(chainID, error, "An exception occured while getting release details");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting release details");
        }
        catch (Exception error) {
            HandleException(chainID, error, "An exception occured while getting release details");
        }

        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return details;
    }

    public async Task<uint> GetTotalFactoryNfts(ChainID chainID, string factory) {
        var timer = Stopwatch.StartNew();
        uint total = 0;

        try {
            _logger.LogDebug("Getting total factory NFTs for {Factory} on {ChainID}", factory, chainID);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(factory)) return 0;

            var contract = web3.Eth.GetContract(ABIs.NftFactory, factory);
            var getFunction = contract.GetFunction("totalNftsCreated");
            total = await getFunction.CallAsync<uint>();
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting total factory NFTs");
            else 
                HandleException(chainID, error, "An exception occured while getting total factory NFTs");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting total factory NFTs");
            else 
                HandleException(chainID, error, "An exception occured while getting total factory NFTs");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting total factory NFTs");
        }
        catch (Exception error) {
            HandleException(chainID, error, "An exception occured while getting total factory NFTs");
        }

        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return total;
    }

    public async Task<FactoryNftDetails> GetFactoryNft(ChainID chainID, string factory, uint id) {
        var timer = Stopwatch.StartNew();
        FactoryNftDetails details = null;

        try {
            _logger.LogDebug("Retrieving factory NFT {ID} from {Factory} on {ChainID}", id, factory, chainID);
            var web3 = GetWeb3(chainID);

            if (web3 == null || string.IsNullOrEmpty(factory)) return null;

            var contract = web3.Eth.GetContract(ABIs.NftFactory, factory);
            var getFunction = contract.GetFunction("nfts");
            var result = await getFunction.CallAsync<DeployedNftResponse>(id);
                
            details = new FactoryNftDetails(result);
        }
        catch (Nethereum.JsonRpc.Client.RpcClientUnknownException error) {
            if (error.InnerException is System.Net.Http.HttpRequestException) 
                HandleTimeout(chainID, "Web3 connection timed out getting factory NFT");
            else 
                HandleException(chainID, error, "An exception occured while getting factory NFT");
        }
        catch (Nethereum.JsonRpc.Client.RpcResponseException error) {
            if (error.Message.Contains("internal error: eth_call")) 
                HandleTimeout(chainID, "Web3 connection timed out getting factory NFT");
            else 
                HandleException(chainID, error, "An exception occured while getting factory NFT");
        }
        catch (Nethereum.JsonRpc.Client.RpcClientTimeoutException) {
            HandleTimeout(chainID, "Web3 connection timed out getting total NFT");
        }
        catch (Exception error) {
            HandleException(chainID, error, "An exception occured while getting factory NFT");
        }

        _stats.Find(a => a.ChainID == chainID)?.AddOperation(timer.ElapsedMilliseconds);
        return details;
    }

    private static string GetMarketAddress(ChainID chainID, int version) {
        return version switch {
            1 => Contracts.OblivionMarket.GetAddress(chainID),
            2 => Contracts.OblivionMarketV2.GetAddress(chainID),
            _ => null
        };
    }

    private static Web3 GetWeb3(ChainID chainID) {
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
            
            case ChainID.Nervos_Mainnet:
                var nervosMainnet = Globals.Blockchains.Find(a => a.ChainID == ChainID.Nervos_Mainnet);
                return nervosMainnet != null ? new Web3(nervosMainnet.Node) { TransactionManager = { UseLegacyAsDefault = true } } : null;

            default: return null;
        }
    }
}