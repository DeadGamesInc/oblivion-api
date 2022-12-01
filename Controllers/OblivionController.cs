/*
 *  OblivionAPI :: OblivionController
 *
 *  This controller defines all the functionality of the API. It is abstract and needs to be inherited by one of the
 *  blockchain specific controllers in order to set the Chain ID, as well as the base controller route.
 * 
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace OblivionAPI.Controllers; 

[ApiController]
public abstract class OblivionController : ControllerBase {
    private readonly ILogger<OblivionController> _logger;
    private readonly DatabaseService _database;
    private readonly ReportsService _reports;
    private readonly ChainID _chainID;

    protected OblivionController(ILogger<OblivionController> logger, DatabaseService database, ReportsService reports, ChainID chainID) {
        _logger = logger;
        _database = database;
        _reports = reports;
        _chainID = chainID;
    }

    [HttpGet]
    [Route("initialSyncComplete")]
    public async Task<ActionResult<bool>> InitialSyncComplete() {
        return Ok(_database.InitialSyncComplete);
    }

    [HttpGet]
    [Route("getTotalListings")]
    public async Task<ActionResult<uint>> GetTotalListings() {
        _logger.LogInformation("getTotalListings for Chain ID {ChainID}", _chainID);
        var listings = await _database.TotalListings(_chainID);
        return Ok(listings);
    }
    
    [HttpGet]
    [Route("getTotalListings1155")]
    public async Task<ActionResult<uint>> GetTotalListings1155() {
        _logger.LogInformation("getTotalListings1155 for Chain ID {ChainID}", _chainID);
        var listings = await _database.TotalListings1155(_chainID);
        return Ok(listings);
    }
        
    [HttpGet]
    [Route("getListings")]
    public async Task<ActionResult<ListListingsDTO[]>> GetListings() {
        _logger.LogInformation("getListings on {ChainID}", _chainID);
        var listings = await _database.GetListings(_chainID);
        if (listings == null) return NotFound(null);
        return Ok(listings.ToArray());
    }
    
    [HttpGet]
    [Route("getListings1155")]
    public async Task<ActionResult<ListListingsDTO[]>> GetListings1155() {
        _logger.LogInformation("getListings1155 on {ChainID}", _chainID);
        var listings = await _database.GetListings1155(_chainID);
        if (listings == null) return NotFound(null);
        return Ok(listings.ToArray());
    }

    [HttpGet]
    [Route("getOpenListings")]
    public async Task<ActionResult<ListListingsDTO[]>> GetOpenListings() {
        _logger.LogInformation("getOpenListings on {ChainID}", _chainID);
        var listings = await _database.GetListings(_chainID);
        if (listings == null) return NotFound(null);
        var open = listings.Where(a => a.SaleState == 0);
        return Ok(open.ToArray());
    }
    
    [HttpGet]
    [Route("getOpenListings1155")]
    public async Task<ActionResult<ListListingsDTO[]>> GetOpenListings1155() {
        _logger.LogInformation("getOpenListings1155 on {ChainID}", _chainID);
        var listings = await _database.GetListings1155(_chainID);
        if (listings == null) return NotFound(null);
        var open = listings.Where(a => a.SaleState == 0);
        return Ok(open.ToArray());
    }

    [HttpGet]
    [Route("getClosedListings")]
    public async Task<ActionResult<ListListingsDTO[]>> GetClosedListings() {
        _logger.LogInformation("getClosedListings on {ChainID}", _chainID);
        var listings = await _database.GetListings(_chainID);
        if (listings == null) return NotFound(null);
        var open = listings.Where(a => a.SaleState == 1);
        return Ok(open.ToArray());
    }
    
    [HttpGet]
    [Route("getClosedListings1155")]
    public async Task<ActionResult<ListListingsDTO[]>> GetClosedListings1155() {
        _logger.LogInformation("getClosedListings1155 on {ChainID}", _chainID);
        var listings = await _database.GetListings1155(_chainID);
        if (listings == null) return NotFound(null);
        var open = listings.Where(a => a.SaleState == 1);
        return Ok(open.ToArray());
    }

    [HttpGet]
    [Route("getSoldListings")]
    public async Task<ActionResult<ListListingsDTO[]>> GetSoldListings() {
        _logger.LogInformation("getSoldListings on {ChainID}", _chainID);
        var listings = await _database.GetListings(_chainID);
        if (listings == null) return NotFound(null);
        var sold = listings.Where(a => a.WasSold);
        return Ok(sold.ToArray());
    }
    
    [HttpGet]
    [Route("getSoldListings1155")]
    public async Task<ActionResult<ListListingsDTO[]>> GetSoldListings1155() {
        _logger.LogInformation("getSoldListings1155 on {ChainID}", _chainID);
        var listings = await _database.GetListings1155(_chainID);
        if (listings == null) return NotFound(null);
        var sold = listings.Where(a => a.WasSold);
        return Ok(sold.ToArray());
    }

    [HttpGet]
    [Route("getListingsByNft/{nft}")]
    public async Task<ActionResult<ListListingsDTO[]>> GetListingsByNft(string nft) {
        _logger.LogInformation("getListingsByNft for {Nft} on {ChainID}", nft, _chainID);
        var listings = await _database.GetListings(_chainID);
        if (listings == null) return NotFound(null);
        var list = listings.Where(a => a.NFT == nft);
        return Ok(list.ToArray());
    }
    
    [HttpGet]
    [Route("getListingsByNft1155/{nft}")]
    public async Task<ActionResult<ListListingsDTO[]>> GetListingsByNft1155(string nft) {
        _logger.LogInformation("getListingsByNft1155 for {Nft} on {ChainID}", nft, _chainID);
        var listings = await _database.GetListings1155(_chainID);
        if (listings == null) return NotFound(null);
        var list = listings.Where(a => a.NFT == nft);
        return Ok(list.ToArray());
    }

    [HttpGet]
    [Route("getOpenListingsByNft/{nft}")]
    public async Task<ActionResult<ListListingsDTO[]>> GetOpenListingsByNft(string nft) {
        _logger.LogInformation("getOpenListingsByNft for {Nft} on {ChainID}", nft, _chainID);
        var listings = await _database.GetListings(_chainID);
        if (listings == null) return NotFound(null);
        var list = listings.Where(a => a.NFT == nft && a.SaleState == 0);
        return Ok(list.ToArray());
    }
    
    [HttpGet]
    [Route("getOpenListingsByNft1155/{nft}")]
    public async Task<ActionResult<ListListingsDTO[]>> GetOpenListingsByNft1155(string nft) {
        _logger.LogInformation("getOpenListingsByNft1155 for {Nft} on {ChainID}", nft, _chainID);
        var listings = await _database.GetListings1155(_chainID);
        if (listings == null) return NotFound(null);
        var list = listings.Where(a => a.NFT == nft && a.SaleState == 0);
        return Ok(list.ToArray());
    }

    [HttpGet]
    [Route("getUserListings/{wallet}")]
    public async Task<ActionResult<ListListingsDTO[]>> GetUserListings(string wallet) {
        _logger.LogInformation("getUserListings for {Wallet} on {ChainID}", wallet, _chainID);
        var listings = await _database.GetListings(_chainID);
        if (listings == null) return NotFound(null);
        var list = listings.Where(a => a.Owner == wallet);
        return Ok(list.ToArray());
    }
    
    [HttpGet]
    [Route("getUserListings1155/{wallet}")]
    public async Task<ActionResult<ListListingsDTO[]>> GetUserListings1155(string wallet) {
        _logger.LogInformation("getUserListings1155 for {Wallet} on {ChainID}", wallet, _chainID);
        var listings = await _database.GetListings1155(_chainID);
        if (listings == null) return NotFound(null);
        var list = listings.Where(a => a.Owner == wallet);
        return Ok(list.ToArray());
    }

    [HttpGet]
    [Route("getUserListingsWithOpenOffers/{wallet}")]
    public async Task<ActionResult<ListListingsDTO[]>> GetUserListingWithOpenOffers(string wallet) {
        _logger.LogInformation("getUserListingsWithOpenOffers for {Wallet} on {ChainID}", wallet, _chainID);
        var listings = await _database.GetListings(_chainID);
        if (listings == null) return NotFound(null);
        var list = listings.Where(a => a.Owner == wallet && a.SaleState == 0 && a.OpenOffers > 0);
        return Ok(list.ToArray());
    }
    
    [HttpGet]
    [Route("getUserListingsWithOpenOffers1155/{wallet}")]
    public async Task<ActionResult<ListListingsDTO[]>> GetUserListingWithOpenOffers1155(string wallet) {
        _logger.LogInformation("getUserListingsWithOpenOffers1155 for {Wallet} on {ChainID}", wallet, _chainID);
        var listings = await _database.GetListings1155(_chainID);
        if (listings == null) return NotFound(null);
        var list = listings.Where(a => a.Owner == wallet && a.SaleState == 0 && a.OpenOffers > 0);
        return Ok(list.ToArray());
    }

    [HttpGet]
    [Route("getUserOpenListings/{wallet}")]
    public async Task<ActionResult<ListListingsDTO[]>> GetUserOpenListings(string wallet) {
        _logger.LogInformation("getUserOpenListings for {Wallet} on {ChainID}", wallet, _chainID);
        var listings = await _database.GetListings(_chainID);
        if (listings == null) return NotFound(null);
        var list = listings.Where(a => a.Owner == wallet && a.SaleState == 0);
        return Ok(list.ToArray());
    }
    
    [HttpGet]
    [Route("getUserOpenListings1155/{wallet}")]
    public async Task<ActionResult<ListListingsDTO[]>> GetUserOpenListings1155(string wallet) {
        _logger.LogInformation("getUserOpenListings1155 for {Wallet} on {ChainID}", wallet, _chainID);
        var listings = await _database.GetListings1155(_chainID);
        if (listings == null) return NotFound(null);
        var list = listings.Where(a => a.Owner == wallet && a.SaleState == 0);
        return Ok(list.ToArray());
    }

    [HttpGet]
    [Route("getUserOffers/{wallet}")]
    public async Task<ActionResult<OfferDetails>> GetUserOffers(string wallet) {
        _logger.LogInformation("getUserOffers for {Wallet} on {ChainID}", wallet, _chainID);
        var offers = await _database.GetUserOffers(_chainID, wallet);
        if (offers == null) return NotFound(null);
        return Ok(offers.ToArray());
    }
    
    [HttpGet]
    [Route("getUserOffers1155/{wallet}")]
    public async Task<ActionResult<OfferDetails>> GetUserOffers1155(string wallet) {
        _logger.LogInformation("getUserOffers1155 for {Wallet} on {ChainID}", wallet, _chainID);
        var offers = await _database.GetUserOffers1155(_chainID, wallet);
        if (offers == null) return NotFound(null);
        return Ok(offers.ToArray());
    }

    [HttpGet]
    [Route("getUserCollections/{wallet}")]
    public async Task<ActionResult<CollectionDetails>> GetUserCollections(string wallet) {
        _logger.LogInformation("getUserCollections for {Wallet} on {ChainID}", wallet, _chainID);
        var collections = await _database.GetCollections(_chainID);
        if (collections == null) return NotFound(null);
        var list = collections.Where(a => a.Owner == wallet);
        return Ok(list.ToArray());
    }
    
    [HttpGet]
    [Route("getUserReleases/{wallet}")]
    public async Task<ActionResult<ReleaseDetails>> GetUserReleases(string wallet) {
        _logger.LogInformation("getUserReleases for {Wallet} on {ChainID}", wallet, _chainID);
        var releases = await _database.GetReleases(_chainID);
        if (releases == null) return NotFound(null);
        var list = releases.Where(a => a.Owner == wallet);
        return Ok(list.ToArray());
    }
    
    [HttpGet]
    [Route("getUserReleases1155/{wallet}")]
    public async Task<ActionResult<ReleaseDetails>> GetUserReleases1155(string wallet) {
        _logger.LogInformation("getUserReleases1155 for {Wallet} on {ChainID}", wallet, _chainID);
        var releases = await _database.GetReleases1155(_chainID);
        if (releases == null) return NotFound(null);
        var list = releases.Where(a => a.Owner == wallet);
        return Ok(list.ToArray());
    }

    [HttpGet]
    [Route("getListing/{version:int}/{id}")]
    public async Task<ActionResult<ListingDetails>> GetListing(int version, uint id) {
        _logger.LogInformation("getListing {ListingID} on {ChainID}", id, _chainID);
        var details = await _database.ListingDetails(_chainID, version, id);
        if (details == null) return NotFound(null);
        return Ok(details);
    }
    
    [HttpGet]
    [Route("getListing1155/{id}")]
    public async Task<ActionResult<ListingDetails>> GetListing1155(uint id) {
        _logger.LogInformation("getListing1155 {ListingID} on {ChainID}", id, _chainID);
        var details = await _database.ListingDetails1155(_chainID, id);
        if (details == null) return NotFound(null);
        return Ok(details);
    }

    [HttpGet]
    [Route("getTotalOffers/{version:int}/{id}")]
    public async Task<ActionResult<uint>> GetTotalOffers(int version, uint id) {
        _logger.LogInformation("getTotalOffers for listing {ID} on {ChainID}", id, _chainID);
        var offers = await _database.TotalOffers(_chainID, id, version);
        return Ok(offers);
    }
    
    [HttpGet]
    [Route("getTotalOffers1155/{id}")]
    public async Task<ActionResult<uint>> GetTotalOffers1155(uint id) {
        _logger.LogInformation("getTotalOffers1155 for listing {ID} on {ChainID}", id, _chainID);
        var offers = await _database.TotalOffers1155(_chainID, id);
        return Ok(offers);
    }
        
    [HttpGet]
    [Route("getOffers/{version:int}/{id}")]
    public async Task<ActionResult<OfferDetails[]>> GetOffers(int version, uint id) {
        _logger.LogInformation("getOffers for {ID} on {ChainID}", id, _chainID);
        var offers = await _database.GetOffers(_chainID, id, version);
        if (offers == null) return NotFound(null);
        return Ok(offers.ToArray());
    }
    
    [HttpGet]
    [Route("getOffers1155/{version:int}/{id}")]
    public async Task<ActionResult<OfferDetails[]>> GetOffers1155(uint id) {
        _logger.LogInformation("getOffers1155 for {ID} on {ChainID}", id, _chainID);
        var offers = await _database.GetOffers1155(_chainID, id);
        if (offers == null) return NotFound(null);
        return Ok(offers.ToArray());
    }

    [HttpGet]
    [Route("getOpenOffers/{version:int}/{id}")]
    public async Task<ActionResult<OfferDetails[]>> GetOpenOffers(int version, uint id) {
        _logger.LogInformation("getOpenOffers for {ID} on {ChainID}", id, _chainID);
        var offers = await _database.GetOffers(_chainID, id, version);
        if (offers == null) return NotFound(null);
        var list = offers.Where(a => !a.Claimed);
        return Ok(list.ToArray());
    }
    
    [HttpGet]
    [Route("getOpenOffers1155/{id}")]
    public async Task<ActionResult<OfferDetails[]>> GetOpenOffers1155(uint id) {
        _logger.LogInformation("getOpenOffers1155 for {ID} on {ChainID}", id, _chainID);
        var offers = await _database.GetOffers1155(_chainID, id);
        if (offers == null) return NotFound(null);
        var list = offers.Where(a => !a.Claimed);
        return Ok(list.ToArray());
    }ƒ

    [HttpGet]
    [Route("getOffer/{version:int}/{id}/{paymentToken}/{offerID}")]
    public async Task<ActionResult<OfferDetails>> GetOffer(int version, uint id, string paymentToken, uint offerID) {
        _logger.LogInformation("getOffer {PaymentToken}:{OfferID} on {ChainID}", id, paymentToken, _chainID);
        var details = await _database.OfferDetails(_chainID, version, id, paymentToken, offerID);
        if (details == null) return NotFound(null);
        return Ok(details);
    }
    
    [HttpGet]
    [Route("getOffer1155/{id}/{paymentToken}/{offerID}")]
    public async Task<ActionResult<OfferDetails>> GetOffer1155(uint id, string paymentToken, uint offerID) {
        _logger.LogInformation("getOffer1155 {PaymentToken}:{OfferID} on {ChainID}", id, paymentToken, _chainID);
        var details = await _database.OfferDetails1155(_chainID, id, paymentToken, offerID);
        if (details == null) return NotFound(null);
        return Ok(details);
    }

    [HttpGet]
    [Route("getSales")]
    public async Task<ActionResult<OblivionSaleInformation[]>> GetSales() {
        _logger.LogInformation("getSales on {ChainID}", _chainID);
        var sales = await _database.GetSales(_chainID);
        if (sales == null) return NotFound(null);
        return Ok(sales.ToArray());
    }
    
    [HttpGet]
    [Route("getSales1155")]
    public async Task<ActionResult<OblivionSaleInformation[]>> GetSales1155() {
        _logger.LogInformation("getSales1155 on {ChainID}", _chainID);
        var sales = await _database.GetSales1155(_chainID);
        if (sales == null) return NotFound(null);
        return Ok(sales.ToArray());
    }

    [HttpGet]
    [Route("getNfts")]
    public async Task<ActionResult<NFTDetails[]>> GetNFTs() {
        _logger.LogInformation("getNfts on {ChainID}", _chainID);
        var nfts = await _database.GetNFTs(_chainID);
        if (nfts == null) return NotFound(null);
        return Ok(nfts.ToArray());
    }
    
    [HttpGet]
    [Route("getNfts1155")]
    public async Task<ActionResult<NFTDetails[]>> GetNFTs1155() {
        _logger.LogInformation("getNfts1155 on {ChainID}", _chainID);
        var nfts = await _database.GetNFT1155s(_chainID);
        if (nfts == null) return NotFound(null);
        return Ok(nfts.ToArray());
    }
        
    [HttpPost]
    [Route("getNftsByAddress")]
    public async Task<ActionResult<NFTDetails[]>> GetNFTSByAddress([FromBody] string[] addresses) {
        _logger.LogInformation("getNftsByAddress on {ChainID}", _chainID);
        var nfts = await _database.GetNFTs(_chainID);
        nfts = (from nft in nfts from check in addresses where string.Equals(nft.Address, check, StringComparison.CurrentCultureIgnoreCase) select nft).ToList();
        return Ok(nfts.ToArray());
    }
    
    [HttpPost]
    [Route("getNftsByAddress1155")]
    public async Task<ActionResult<NFTDetails[]>> GetNFTSByAddress1155([FromBody] string[] addresses) {
        _logger.LogInformation("getNftsByAddress1155 on {ChainID}", _chainID);
        var nfts = await _database.GetNFT1155s(_chainID);
        nfts = (from nft in nfts from check in addresses where string.Equals(nft.Address, check, StringComparison.CurrentCultureIgnoreCase) select nft).ToList();
        return Ok(nfts.ToArray());
    }

    [HttpGet]
    [Route("getNft/{address}")]
    public async Task<ActionResult<NFTDetails>> GetNFT(string address) {
        _logger.LogInformation("getNft for {Address} on {ChainID}", address, _chainID);
        var nft = await _database.NFTDetails(_chainID, address);
        if (nft == null) return NotFound(null);
        return Ok(nft);
    }
    
    [HttpGet]
    [Route("getNft1155/{address}")]
    public async Task<ActionResult<NFTDetails>> GetNFT1155(string address) {
        _logger.LogInformation("getNft1155 for {Address} on {ChainID}", address, _chainID);
        var nft = await _database.NFTDetails1155(_chainID, address);
        if (nft == null) return NotFound(null);
        return Ok(nft);
    }

    [HttpGet]
    [Route("getNftTokenURI/{address}/{tokenID}")]
    public async Task<ActionResult<NFTTokenIDInfo>> GetNFTTokenURI(string address, uint tokenID) {
        _logger.LogInformation("getNftTokenURI for {Address}:{TokenID} on {ChainID}", address, tokenID, _chainID);
        var tokenURI = await _database.NFTTokenURI(_chainID, address, tokenID);
        if (tokenURI == null) return NotFound(null);
        return Ok(tokenURI);
    }
    
    [HttpGet]
    [Route("getNftTokenURI1155/{address}/{tokenID}")]
    public async Task<ActionResult<NFTTokenIDInfo>> GetNFTTokenURI1155(string address, uint tokenID) {
        _logger.LogInformation("getNftTokenURI1155 for {Address}:{TokenID} on {ChainID}", address, tokenID, _chainID);
        var tokenURI = await _database.NFTTokenURI1155(_chainID, address, tokenID);
        if (tokenURI == null) return NotFound(null);
        return Ok(tokenURI);
    }
        
    [HttpPost]
    [Route("getNftTokenURIs/{address}")]
    public async Task<ActionResult<NFTTokenIDInfo[]>> GetNFTTokenURIs(string address, [FromBody] uint[] tokenIDs) {
        _logger.LogInformation("getNftTokenURIs for {Address} on {ChainID}", address, _chainID);
        var tokenURIs = new List<NFTTokenIDInfo>();

        foreach (var id in tokenIDs) {
            var tokenURI = await _database.NFTTokenURI(_chainID, address, id);
            if (tokenURI != null) tokenURIs.Add(tokenURI);
        }
            
        return Ok(tokenURIs.ToArray());
    }
    
    [HttpPost]
    [Route("getNft1155TokenURIs/{address}")]
    public async Task<ActionResult<NFTTokenIDInfo[]>> GetNFT1155TokenURIs(string address, [FromBody] uint[] tokenIDs) {
        _logger.LogInformation("getNft1155TokenURIs for {Address} on {ChainID}", address, _chainID);
        var tokenURIs = new List<NFTTokenID1155Info>();

        foreach (var id in tokenIDs) {
            var tokenURI = await _database.NFTTokenURI1155(_chainID, address, id);
            if (tokenURI != null) tokenURIs.Add(tokenURI);
        }
            
        return Ok(tokenURIs.ToArray());
    }

    [HttpGet]
    [Route("getTotalCollections")]
    public async Task<ActionResult<uint>> GetTotalCollections() {
        _logger.LogInformation("getTotalCollections on {ChainID}", _chainID);
        var collections = await _database.TotalCollections(_chainID);
        return collections;
    }

    [HttpGet]
    [Route("getCollections")]
    public async Task<ActionResult<CollectionDetails[]>> GetCollections() {
        _logger.LogInformation("getCollections on {ChainID}", _chainID);
        var collections = await _database.GetCollections(_chainID);
        if (collections == null) return NotFound(null);
        return Ok(collections.ToArray());
    }

    [HttpGet]
    [Route("getCollection/{id}")]
    public async Task<ActionResult<CollectionDetails>> GetCollection(uint id) {
        _logger.LogInformation("getCollection {ID} on {ChainID}", id, _chainID);
        var collection = await _database.CollectionDetails(_chainID, id);
        if (collection == null) return NotFound(null);
        return Ok(collection);
    }
        
    [HttpGet]
    [Route("getTotalReleases")]
    public async Task<ActionResult<uint>> GetTotalReleases() {
        _logger.LogInformation("getTotalReleases on {ChainID}", _chainID);
        var releases = await _database.TotalReleases(_chainID);
        return releases;
    }
    
    [HttpGet]
    [Route("getTotalReleases1155")]
    public async Task<ActionResult<uint>> GetTotalReleases1155() {
        _logger.LogInformation("getTotalReleases1155 on {ChainID}", _chainID);
        var releases = await _database.TotalReleases1155(_chainID);
        return releases;
    }

    [HttpGet]
    [Route("getReleases")]
    public async Task<ActionResult<ReleaseDetails[]>> GetReleases() {
        _logger.LogInformation("getReleases on {ChainID}", _chainID);
        var releases = await _database.GetReleases(_chainID);
        if (releases == null) return NotFound(null);
        return Ok(releases.ToArray());
    }
    
    [HttpGet]
    [Route("getReleases1155")]
    public async Task<ActionResult<ReleaseDetails[]>> GetReleases1155() {
        _logger.LogInformation("getReleases1155 on {ChainID}", _chainID);
        var releases = await _database.GetReleases1155(_chainID);
        if (releases == null) return NotFound(null);
        return Ok(releases.ToArray());
    }

    [HttpGet]
    [Route("getRelease/{id}")]
    public async Task<ActionResult<ReleaseDetails>> GetRelease(uint id) {
        _logger.LogInformation("getRelease {ID} on {ChainID}", id, _chainID);
        var release = await _database.ReleaseDetails(_chainID, id);
        if (release == null) return NotFound(null);
        return Ok(release);
    }
    
    [HttpGet]
    [Route("getRelease1155/{id}")]
    public async Task<ActionResult<ReleaseDetails>> GetRelease1155(uint id) {
        _logger.LogInformation("getRelease1155 {ID} on {ChainID}", id, _chainID);
        var release = await _database.ReleaseDetails1155(_chainID, id);
        if (release == null) return NotFound(null);
        return Ok(release);
    }

    [HttpGet]
    [Route("getPaymentTokens")]
    public async Task<ActionResult<List<PaymentTokenDetails>>> GetPaymentTokens() {
        _logger.LogInformation("getPaymentTokens on {ChainID}", _chainID);
        var tokens = await DatabaseService.GetPaymentTokens(_chainID);
        if (tokens == null) return NotFound(null);
        return Ok(tokens.ToArray());
    }

    [HttpGet]
    [Route("refreshListing/{version:int}/{id}")]
    public async Task<ActionResult<ListingDetails>> RefreshListing(int version, uint id) {
        _logger.LogInformation("refreshListing for {ID} on {ChainID}", id, _chainID);
        var listing = await _database.RefreshListing(_chainID, version, id);
        if (listing == null) return NotFound(null);
        return Ok(listing);
    }
    
    [HttpGet]
    [Route("refreshListing1155/{id}")]
    public async Task<ActionResult<ListingDetails>> RefreshListing1155(uint id) {
        _logger.LogInformation("refreshListing1155 for {ID} on {ChainID}", id, _chainID);
        var listing = await _database.RefreshListing1155(_chainID, id);
        if (listing == null) return NotFound(null);
        return Ok(listing);
    }

    [HttpGet]
    [Route("refreshOffer/{version:int}/{listingId}/{paymentToken}/{id}")]
    public async Task<ActionResult<OfferDetails>> RefreshOffer(int version, uint listingId, string paymentToken, uint id) {
        _logger.LogInformation("refreshOffer {ListingID}:{PaymentToken}:{OfferID} on {ChainID}", listingId, paymentToken, id, _chainID);
        var offer = await _database.RefreshOffer(_chainID, version, listingId, paymentToken, id);
        if (offer == null) return NotFound(null);
        return Ok(offer);
    }
    
    [HttpGet]
    [Route("refreshOffer1155/{listingId}/{paymentToken}/{id}")]
    public async Task<ActionResult<OfferDetails>> RefreshOffer1155(uint listingId, string paymentToken, uint id) {
        _logger.LogInformation("refreshOffer {ListingID}:{PaymentToken}:{OfferID} on {ChainID}", listingId, paymentToken, id, _chainID);
        var offer = await _database.RefreshOffer1155(_chainID, listingId, paymentToken, id);
        if (offer == null) return NotFound(null);
        return Ok(offer);
    }

    [HttpGet]
    [Route("refreshCollection/{id}")]
    public async Task<ActionResult<CollectionDetails>> RefreshCollection(uint id) {
        _logger.LogInformation("refreshCollection {ID} on {ChainID}", id, _chainID);
        var collection = await _database.RefreshCollection(_chainID, id);
        if (collection == null) return NotFound(null);
        return Ok(collection);
    }

    [HttpGet]
    [Route("refreshRelease/{id}")]
    public async Task<ActionResult<ReleaseDetails>> RefreshRelease(uint id) {
        _logger.LogInformation("refreshRelease {ID} on {ChainId}", id, _chainID);
        var release = await _database.RefreshRelease(_chainID, id);
        if (release == null) return NotFound(null);
        return Ok(release);
    }
    
    [HttpGet]
    [Route("refreshRelease1155/{id}")]
    public async Task<ActionResult<ReleaseDetails>> RefreshRelease1155(uint id) {
        _logger.LogInformation("refreshRelease1155 {ID} on {ChainId}", id, _chainID);
        var release = await _database.RefreshRelease1155(_chainID, id);
        if (release == null) return NotFound(null);
        return Ok(release);
    }
        
    [HttpGet]
    [Route("refreshNft/{address}")]
    public async Task<ActionResult<NFTDetails>> RefreshNft(string address) {
        _logger.LogInformation("refreshNft for {Address} on {ChainID}", address, _chainID);
        var nft = await _database.RefreshNft(_chainID, address);
        if (nft == null) return NotFound(null);
        return Ok(nft);
    }
    
    [HttpGet]
    [Route("refreshNft1155/{address}")]
    public async Task<ActionResult<NFTDetails>> RefreshNft1155(string address) {
        _logger.LogInformation("refreshNft1155 for {Address} on {ChainID}", address, _chainID);
        var nft = await _database.RefreshNft1155(_chainID, address);
        if (nft == null) return NotFound(null);
        return Ok(nft);
    }

    [HttpGet]
    [Route("recacheNft/{address}")]
    public async Task<ActionResult<NFTDetails>> RecacheNft(string address) {
        _logger.LogInformation("recacheNft for {Address} on {ChainID}", address, _chainID);
        var nft = await _database.RecacheNft(_chainID, address);
        if (nft == null) return NotFound(null);
        return Ok(nft);
    }
    
    [HttpGet]
    [Route("recacheNft1155/{address}")]
    public async Task<ActionResult<NFTDetails>> RecacheNft1155(string address) {
        _logger.LogInformation("recacheNft1155 for {Address} on {ChainID}", address, _chainID);
        var nft = await _database.RecacheNft1155(_chainID, address);
        if (nft == null) return NotFound(null);
        return Ok(nft);
    }

    [HttpGet]
    [Route("get24HourVolume")]
    public async Task<ActionResult<SalesReport_Volume>> SalesReport_24HVolume() {
        _logger.LogInformation("get24HourVolume on {ChainID}", _chainID);
        var report = await _reports.SalesReport_24HVolume(_chainID);
        if (report == null) return NotFound(null);
        return Ok(report);
    }
        
    [HttpGet]
    [Route("get30DayVolume")]
    public async Task<ActionResult<SalesReport_Volume>> SalesReport_30DVolume() {
        _logger.LogInformation("get30DayVolume on {ChainID}", _chainID);
        var report = await _reports.SalesReport_30DVolume(_chainID);
        if (report == null) return NotFound(null);
        return Ok(report);
    }
        
    [HttpGet]
    [Route("getMonthlyVolume")]
    public async Task<ActionResult<SalesReport_Volume>> SalesReport_MonthlyVolume() {
        _logger.LogInformation("getMonthlyVolume on {ChainID}", _chainID);
        var report = await _reports.SalesReport_CurrentMonthVolume(_chainID);
        if (report == null) return NotFound(null);
        return Ok(report);
    }
        
    [HttpGet]
    [Route("getPreviousMonthlyVolume")]
    public async Task<ActionResult<SalesReport_Volume>> SalesReport_PreviousMonthlyVolume() {
        _logger.LogInformation("getPreviousMonthlyVolume on {ChainID}", _chainID);
        var report = await _reports.SalesReport_PreviousMonthVolume(_chainID);
        if (report == null) return NotFound(null);
        return Ok(report);
    }

    [HttpGet]
    [Route("status")]
    public async Task<ActionResult<string>> Status() {
        _logger.LogInformation("status request");
        var status = await _database.GetStatus();
        return Ok(status);
    }
}