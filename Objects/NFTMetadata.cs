/*
 *  OblivionAPI :: NFTMetadata
 *
 *  This class is used to store NFT metadata.
 * 
 */

namespace OblivionAPI.Objects; 

[Serializable]
public class NFTMetadata {
    public string Name { get; set; }
    public string Description { get; set; }
    public string ExternalUrl { get; set; }
    public string Image { get; set; }
    public List<NFTMetadataTrait> Attributes { get; set; } = new();

    public NFTMetadata(NftMetadataResponse response) {
        Name = response.name;
        Description = response.description;
        ExternalUrl = response.external_url;
        Image = response.image;

        if (response.attributes != null) foreach (var trait in response.attributes) Attributes.Add(new NFTMetadataTrait(trait));
    }
}