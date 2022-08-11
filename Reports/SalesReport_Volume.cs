namespace OblivionAPI.Reports; 

[Serializable]
public class SalesReport_Volume {
    public int TotalSales { get; set; }
    public int TotalSales1155 { get; set; }
    public int TotalReleaseSales { get; set; }
    public int TotalReleaseSales1155 { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal TotalVolume1155 { get; set; }
    public decimal TotalReleaseVolume { get; set; }
    public decimal TotalReleaseVolume1155 { get; set; }

    public List<SalesReport_CollectionVolume> Collections { get; set; } = new();
    public List<SalesReport_CollectionVolume> Collections1155 { get; set; } = new();
    public List<SalesReport_ReleaseVolume> Releases { get; set; } = new();
    public List<SalesReport_ReleaseVolume> Releases1155 { get; set; } = new();
}