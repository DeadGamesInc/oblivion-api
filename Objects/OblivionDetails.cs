/*
 *  OblivionAPI :: OblivionDetails
 *
 *  This class is used to store all of the details for a given blockchain in the database.
 * 
 */

using System.Collections.Generic;

namespace OblivionAPI.Objects; 

public class OblivionDetails {
    public ChainID ChainID;
    public uint ReleaseStartingBlock = 0;
    public uint LastReleaseScannedBlock = 0;
        
    public uint TotalListings;
    public uint TotalListingsV1;
    public uint TotalListingsV2;
    public uint TotalCollections;
    public uint TotalReleases;
        
    public readonly List<ListingDetails> Listings = new();
    public readonly List<CollectionDetails> Collections = new();
    public readonly List<ReleaseDetails> Releases = new();
    public readonly List<NFTDetails> NFTs = new();
    public readonly List<ReleaseSaleDetails> ReleaseSales = new();
}