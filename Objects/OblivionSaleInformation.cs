using System;

namespace OblivionAPI.Objects {
    public class OblivionSaleInformation {
        public uint ID { get; set; }
        public string Amount { get; set; }
        public string PaymentToken { get; set; }
        public string Buyer { get; set; }
        public string Seller { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime SaleDate { get; set; }
    }
}
