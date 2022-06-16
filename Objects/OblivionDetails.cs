/*
 *  OblivionAPI :: OblivionDetails
 *
 *  This class is used to store all of the details for a given blockchain in the database.
 * 
 */

using System.Text;

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

    public bool ListingsUpdated;
    public bool CollectionsUpdated;
    public bool ReleasesUpdated;
    public bool TokensUpdated;
    public bool SaleCollectionsUpdated;
    public bool ReleaseSalesUpdated;
    public bool ListingCollectionsUpdated;
    public bool IPFSUpdated;
    
    public void ClearStatus() {
        ListingsUpdated = false;
        CollectionsUpdated = false;
        ReleasesUpdated = false;
        TokensUpdated = false;
        SaleCollectionsUpdated = false;
        ReleaseSalesUpdated = false;
        ListingCollectionsUpdated = false;
        IPFSUpdated = false;
    }

    public void AddStatus(StringBuilder builder) {
        builder.AppendLine($"Status For Chain            : {ChainID}");
        builder.AppendLine("===============================================");
        builder.AppendLine($"Listings Updated            : {ListingsUpdated}");
        builder.AppendLine($"Collections Updated         : {CollectionsUpdated}");
        builder.AppendLine($"Releases Updated            : {ReleasesUpdated}");
        builder.AppendLine($"Tokens Updated              : {TokensUpdated}");
        builder.AppendLine($"Sales Collections Updated   : {SaleCollectionsUpdated}");
        builder.AppendLine($"Release Sales Updated       : {ReleaseSalesUpdated}");
        builder.AppendLine($"Listing Collections Updated : {ListingCollectionsUpdated}");
        builder.AppendLine($"IPFS Updated                : {IPFSUpdated}");
        builder.AppendLine($"Last Release Scanned Block  : {LastReleaseScannedBlock}");
        builder.AppendLine("");
    }
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