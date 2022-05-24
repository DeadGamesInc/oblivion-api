using System;
using System.Collections.Generic;

namespace OblivionAPI.Reports; 

[Serializable]
public class SalesReport_Volume {
    public int TotalSales { get; set; }
    public int TotalReleaseSales { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal TotalReleaseVolume { get; set; }

    public List<SalesReport_CollectionVolume> Collections { get; set; } = new();
    public List<SalesReport_ReleaseVolume> Releases { get; set; } = new();
}