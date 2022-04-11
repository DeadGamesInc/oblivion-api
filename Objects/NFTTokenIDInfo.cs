/*
 *  OblivionAPI :: NFTTokenIDInfo
 *
 *  This class is used to store the token ID specific details for a NFT.
 * 
 */

namespace OblivionAPI.Objects {
    public class NFTTokenIDInfo {
        public uint TokenId { get; set; }
        public string URI { get; set; }
        public NFTMetadata Metadata { get; set; }
    }
}
