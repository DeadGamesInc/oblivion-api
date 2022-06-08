/*
 *  OblivionAPI :: OblivionDetails
 *
 *  This class is used to store all of the details for a given blockchain in the database.
 * 
 */

namespace OblivionAPI.Objects; 

[Serializable]
public class OblivionDetails {
    public ChainID ChainID { get; set; }
    public uint ReleaseStartingBlock { get; set; }
    public uint LastReleaseScannedBlock { get; set; }
        
    public uint TotalListings { get; set; }
    public uint TotalListingsV1 { get; set; }
    public uint TotalListingsV2 { get; set; }
    public uint TotalCollections { get; set; }
    public uint TotalReleases { get; set; }
        
    public List<ListingDetails> Listings { get; set; } = new();
    public List<CollectionDetails> Collections { get; set; } = new();
    public List<ReleaseDetails> Releases { get; set; } = new();
    public List<NFTDetails> NFTs { get; set; } = new();
    public List<ReleaseSaleDetails> ReleaseSales { get; set; } = new();
}

public class BSCMainnetDefaults : OblivionDetails {
    public BSCMainnetDefaults() {
        ChainID = ChainID.BSC_Mainnet;
        ReleaseStartingBlock = 16636640;
    }
}

public class BSCTestnetDefaults : OblivionDetails {
    public BSCTestnetDefaults() {
        ChainID = ChainID.BSC_Testnet;
        ReleaseStartingBlock = 17931172;
    }
}

public class NervosTestnetDefaults : OblivionDetails {
    public NervosTestnetDefaults() {
        ChainID = ChainID.Nervos_Testnet;
        ReleaseStartingBlock = 87064;
    }
}