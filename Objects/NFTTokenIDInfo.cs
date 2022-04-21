/*
 *  OblivionAPI :: NFTTokenIDInfo
 *
 *  This class is used to store the token ID specific details for a NFT.
 * 
 */

using System;

namespace OblivionAPI.Objects {
    [Serializable]
    public class NFTTokenIDInfo {
        public uint TokenId { get; set; }
        public string URI { get; set; }
        public NFTMetadata Metadata { get; set; }
    }
}
