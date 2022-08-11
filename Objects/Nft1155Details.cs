namespace OblivionAPI.Objects; 

public class Nft1155Details {
    public string Address { get; set; }
    public string BaseURI { get; set; }
    
    public List<NFTTokenID1155Info> TokenDetails { get; set; } = new();
}
