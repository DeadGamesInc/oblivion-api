namespace OblivionAPI.Objects; 

[Serializable]
public class NftMetadataResponse {
    public string name { get; set; }
    public string description { get; set; }
    public string external_url { get; set; }
    public string image { get; set; }
    public NftMetadataTraitResponse[] attributes { get; set; }
}

[Serializable]
public class NftMetadataTraitResponse {
    public string trait_type { get; set; }
    public string value { get; set; }
}