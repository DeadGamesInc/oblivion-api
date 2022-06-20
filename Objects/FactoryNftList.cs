namespace OblivionAPI.Objects; 

[Serializable]
public class FactoryNftList {
    public string Contract { get; set; }
    public List<FactoryNftDetails> Nfts { get; set; } = new();
}
