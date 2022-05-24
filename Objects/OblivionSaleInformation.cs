namespace OblivionAPI.Objects; 

[Serializable]
public class OblivionSaleInformation {
    public uint ID { get; set; }
    public int Version { get; set; }
    public string Amount { get; set; }
    public string PaymentToken { get; set; }
    public string Buyer { get; set; }
    public string Seller { get; set; }
    public string Nft { get; set; }
    public uint? CollectionId { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime SaleDate { get; set; }
    public string TxHash;
    public bool Cancelled;
}