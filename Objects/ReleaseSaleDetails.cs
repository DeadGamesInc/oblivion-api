using System;

namespace OblivionAPI.Objects; 

[Serializable]
public class ReleaseSaleDetails {
    public uint ID { get; set; }
    public int Quantity { get; set; }
    public string Price { get; set; }
    public string PaymentToken { get; set; }
    public DateTime SaleTime { get; set; }
}