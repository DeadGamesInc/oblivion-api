using System;
using System.Linq;

namespace OblivionAPI.Objects; 

[Serializable]
public class ListListingsDTO {
    public uint ID { get; set; }
    public int Version { get; set; }
    public string PaymentToken { get; set; }
    public string NFT { get; set; }
    public string TargetPrice { get; set; }
    public string MinimumPrice { get; set; }
    public uint TokenId { get; set; }
    public string SaleEnd { get; set; }
    public uint PaymentMethod { get; set; }
    public uint SaleType { get; set; }
    public uint SaleState { get; set; }
    public string TopOfferToken { get; set; }
    public string TopOfferAmount { get; set; }
    public string NftName { get; set; }
    public string NftCacheHighRes { get; set; }
    public string NftCacheLowRes { get; set; }
    public uint? CollectionId { get; set; }
    public string CollectionName { get; set; }
    public bool WasSold;
    public string Owner;
    public int OpenOffers;

    public ListListingsDTO(ListingDetails listing, NFTDetails nft) {
        ID = listing.ID;
        Version = listing.Version;
        PaymentMethod = listing.PaymentMethod;
        NFT = listing.NFT;
        PaymentToken = listing.PaymentToken;
        TargetPrice = listing.TargetPrice;
        MinimumPrice = listing.MinimumPrice;
        TokenId = listing.TokenId;
        SaleEnd = listing.SaleEnd;
        SaleType = listing.SaleType;
        SaleState = listing.SaleState;
        WasSold = listing.WasSold;
        Owner = listing.Owner;
        CollectionId = listing.CollectionId;
        CollectionName = listing.CollectionName;
        OpenOffers = listing.Offers.Count(a => !a.Claimed);
        TopOfferAmount = listing.TopOffer?.Amount;
        TopOfferToken = listing.TopOffer?.PaymentToken;
        NftName = nft?.Name;
        var token = nft?.TokenDetails.Find(a => a.TokenId == listing.TokenId);
        NftCacheHighRes = token?.CacheHighRes;
        NftCacheLowRes = token?.CacheLowRes;
    }
}
