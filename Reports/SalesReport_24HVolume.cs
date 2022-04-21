using System.Collections.Generic;

namespace OblivionAPI.Reports {
    public class SalesReport_24HVolume {
        public int TotalSales { get; set; }
        public decimal TotalVolume { get; set; }

        public List<SalesReport_CollectionVolume> Collections { get; set; } = new();
    }
}
