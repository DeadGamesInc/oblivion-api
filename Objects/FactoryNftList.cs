namespace OblivionAPI.Objects; 

[Serializable]
public class FactoryNftList {
    public string Contract { get; set; }
    public List<FactoryNftDetails> Nfts { get; set; } = new();

    public bool is1155(ChainID chainId) {
        return Contract == Contracts.Nft1155Factories[chainId];
    }
}
