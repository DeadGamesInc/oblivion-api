/*
 *  OblivionAPI :: OfferDetails
 *
 *  This class is used to store the details of an offer made on a listing on the market.
 * 
 */

namespace OblivionAPI.Objects; 

[Serializable]
public class OfferDetails {
    public string PaymentToken { get; set; }
    public uint ListingId { get; set; }
    public uint ID { get; set; }
    public int Version { get; set; }
    public uint TokenId { get; set; }
    public string NftSymbol { get; set; }
    public DateTime LastRetrieved;
    public string Offeror { get; set; }
    public string Amount { get; set; }
    public string Discount { get; set; }
    public bool Claimed { get; set; }
    public string CreateBlock { get; set; }
    public string EndBlock { get; set; }

    public OfferDetails(string paymentToken, uint listingId, uint id, int version, OfferResponse response) {
        PaymentToken = paymentToken;
        ID = id;
        ListingId = listingId;
        Version = version;
        Offeror = response.Offeror;
        Amount = response.Amount.ToString();
        Discount = response.Discount.ToString();
        Claimed = response.Claimed;
        CreateBlock = response.CreateBlock.ToString();
        EndBlock = response.EndBlock.ToString();
        LastRetrieved = DateTime.Now;
    }

    public void Update(OfferDetails response) {
        Claimed = response.Claimed;
        EndBlock = response.EndBlock;
        LastRetrieved = DateTime.Now;
    }
}