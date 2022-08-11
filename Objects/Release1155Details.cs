namespace OblivionAPI.Objects; 

public class Release1155Details {
    public uint ID { get; set; }
    public DateTime LastRetrieved;
    public string Owner { get; set; }
    public string NFT { get; set; }
    public uint TokenId { get; set; }
    public string PaymentToken { get; set; }
    public string Price { get; set; }
    public uint Sales { get; set; }
    public uint MaxSales { get; set; }
    public uint MaxQuantity { get; set; }
    public uint EndDate { get; set; }
    public uint Discount { get; set; }
    public bool Whitelisted { get; set; }
    public bool Ended { get; set; }
    public string[] TreasuryAddresses { get; set; }
    public uint[] TreasuryAllocations { get; set; }

    public Release1155Details() {}
    public Release1155Details(uint id, Release1155Response response, ReleaseTreasuryDetailsResponse treasury) {
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
        Whitelisted = response.Whitelisted;
        Ended = response.Ended;
        TokenId = response.TokenId;
        TreasuryAddresses = treasury.TreasuryAddresses.ToArray();
        TreasuryAllocations = treasury.TreasuryAllocations.ToArray();
        LastRetrieved = DateTime.Now;
    }

    public void Update(Release1155Details release) {
        PaymentToken = release.PaymentToken;
        Price = release.Price;
        Sales = release.Sales;
        MaxSales = release.MaxSales;
        MaxQuantity = release.MaxQuantity;
        EndDate = release.EndDate;
        Discount = release.Discount;
        Whitelisted = release.Whitelisted;
        Ended = release.Ended;
        TreasuryAddresses = release.TreasuryAddresses;
        TreasuryAllocations = release.TreasuryAllocations;
        LastRetrieved = DateTime.Now;
    }
}
