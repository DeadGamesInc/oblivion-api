/*
 *  OblivionAPI :: NFTDetails
 *
 *  This class is used to store the details of a NFT that is listed in the market.
 * 
 */

using System;
using System.Collections.Generic;

namespace OblivionAPI.Objects {
    [Serializable]
    public class NFTDetails {
        public string Address { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string URI { get; set; }
        public string CacheHighRes { get; set; }
        public string CacheLowRes { get; set; }
        public NFTMetadata Metadata { get; set; }

        public readonly List<NFTTokenIDInfo> TokenDetails = new();
    }
}
