/*
 *  OblivionAPI :: ListingDetails
 *
 *  This class is used to store the details of a listing on the market.
 * 
 */

namespace OblivionAPI.Objects; 

[Serializable]
public class ListingDetails {
    public uint ID { get; set; }
    public int Version { get; set; }
    public DateTime LastRetrieved;
    public string Owner { get; set; }
    public string PaymentToken { get; set; }
    public string NFT { get; set; }
    public string TargetPrice { get; set; }
    public string MinimumPrice { get; set; }
    public uint TokenId { get; set; }
    public string SaleEnd { get; set; }
    public uint GraceEnd { get; set; }
    public uint CreateBlock { get; set; }
    public uint ClosedBlock { get; set; }
    public uint PaymentMethod { get; set; }
    public uint SaleType { get; set; }
    public uint SaleState { get; set; }
    public OfferDetails? TopOffer { get; set; }
    public string TxHash { get; set; }

    public uint? CollectionId { get; set; }
    public string CollectionName { get; set; }
    
    public bool Finalized{ get; set; }
    public bool WasSold { get; set; }
    public OblivionSaleInformation SaleInformation { get; set; }

    public List<OfferDetails> Offers { get; set; } = new();
    
    public ListingDetails() {}
    public ListingDetails(uint id, int version, ListingResponse response) {
        ID = id;
        Version = version;
        Owner = response.Owner;
        PaymentToken = response.PaymentToken;
        NFT = response.NFT;
        TargetPrice = response.TargetPrice.ToString();
        MinimumPrice = response.MinimumPrice.ToString();
        TokenId = response.TokenID;
        SaleEnd = response.SaleEnd.ToString();
        GraceEnd = response.GraceEnd;
        CreateBlock = response.CreateBlock;
        ClosedBlock = response.ClosedBlock;
        PaymentMethod = response.PaymentMethod;
        SaleType = response.SaleType;
        SaleState = response.SaleState;
        LastRetrieved = DateTime.Now;
    }

    public void Update(ListingDetails response) {
        if (response == null) return;
        ClosedBlock = response.ClosedBlock;
        SaleState = response.SaleState;
        LastRetrieved = DateTime.Now;
    }
}