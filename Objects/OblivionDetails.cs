/*
 *  OblivionAPI :: OblivionDetails
 *
 *  This class is used to store all of the details for a given blockchain in the database.
 * 
 */

using System;
using System.Collections.Generic;

namespace OblivionAPI.Objects {
    public class OblivionDetails {
        public ChainID ChainID;
        public DateTime LastRetrieved = DateTime.MinValue;
        public uint TotalListings;
        public uint TotalListingsV1;
        public uint TotalListingsV2;
        public uint TotalCollections;
        public uint TotalReleases;
        
        public List<ListingDetails> Listings = new();
        public List<CollectionDetails> Collections = new();
        public List<ReleaseDetails> Releases = new();
        public List<NFTDetails> NFTs = new();
    }
}
