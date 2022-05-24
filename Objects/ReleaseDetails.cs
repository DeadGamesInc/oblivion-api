/*
 *  OblivionAPI :: PaymentDetails
 *
 *  This class is used to store the details of an NFT release on the market.
 * 
 */

namespace OblivionAPI.Objects; 

public class ReleaseDetails {
    public uint ID { get; set; }
    public DateTime LastRetrieved;
    public string Owner { get; set; }
    public string NFT { get; set; }
    public string PaymentToken { get; set; }
    public string Price { get; set; }
    public uint Sales { get; set; }
    public uint MaxSales { get; set; }
    public uint MaxQuantity { get; set; }
    public uint EndDate { get; set; }
    public uint Discount { get; set; }
    public bool Selectable { get; set; }
    public bool Whitelisted { get; set; }
    public bool Ended { get; set; }
    public bool UsesReviveRug { get; set; }
    public string[] TreasuryAddresses { get; set; }
    public uint[] TreasuryAllocations { get; set; }

    public ReleaseDetails(uint id, ReleaseResponse response, ReleaseTreasuryDetailsResponse treasury) {
        ID = id;
        Owner = response.Owner;
        NFT = response.NFT;
        PaymentToken = response.PaymentToken;
        Price = response.Price.ToString();
        Sales = response.Sales;
        MaxSales = response.MaxSales;
        MaxQuantity = response.MaxQuantity;
        EndDate = response.EndDate;
        Discount = response.Discount;
        Selectable = response.Selectable;
        Whitelisted = response.Whitelisted;
        Ended = response.Ended;
        UsesReviveRug = response.UsesReviveRug;
        TreasuryAddresses = treasury.TreasuryAddresses.ToArray();
        TreasuryAllocations = treasury.TreasuryAllocations.ToArray();
        LastRetrieved = DateTime.Now;
    }

    public void Update(ReleaseDetails release) {
        PaymentToken = release.PaymentToken;
        Price = release.Price;
        Sales = release.Sales;
        MaxSales = release.MaxSales;
        MaxQuantity = release.MaxQuantity;
        EndDate = release.EndDate;
        Discount = release.Discount;
        Selectable = release.Selectable;
        Whitelisted = release.Whitelisted;
        Ended = release.Ended;
        UsesReviveRug = release.UsesReviveRug;
        TreasuryAddresses = release.TreasuryAddresses;
        TreasuryAllocations = release.TreasuryAllocations;
        LastRetrieved = DateTime.Now;
    }
}