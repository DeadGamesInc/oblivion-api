namespace OblivionAPI.Objects; 

[Serializable]
public class FactoryNftDetails {
    public string Address { get; set; }
    public string Deployer { get; set; }
    public DateTime Deployed { get; set; }
    public string MetadataUri { get; set; }
    public string ImageUri { get; set; }

    public FactoryNftDetails(){}
    public FactoryNftDetails(DeployedNftResponse response) {
        Address = response.Nft;
        Deployer = response.Deployer;
        Deployed = DateTimeOffset.FromUnixTimeSeconds(response.Created).UtcDateTime;
    }
}
