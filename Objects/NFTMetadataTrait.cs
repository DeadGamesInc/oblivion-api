/*
 *  OblivionAPI :: NFTMetadataTrait
 *
 *  This class is used to store NFT trait attributes within the metadata.
 * 
 */

namespace OblivionAPI.Objects; 

[Serializable]
public class NFTMetadataTrait {
    public string TraitType { get; set; }
    public string Value { get; set; }

    public NFTMetadataTrait() {}
    public NFTMetadataTrait(NftMetadataTraitResponse response) {
        TraitType = response.trait_type;
        Value = response.value;
    }
}