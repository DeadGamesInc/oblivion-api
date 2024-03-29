﻿/*
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
    public uint Release1155StartingBlock { get; set; }
    public uint LastReleaseScannedBlock { get; set; }
    public uint LastRelease1155ScannedBlock { get; set; }
        
    public uint TotalListings { get; set; }
    public uint TotalListingsV1 { get; set; }
    public uint TotalListingsV2 { get; set; }
    public uint TotalCollections { get; set; }
    public uint TotalReleases { get; set; }
    public uint Total1155Listings { get; set; }
    public uint TotalReleases1155 { get; set; }
        
    public List<ListingDetails> Listings { get; set; } = new();
    public List<CollectionDetails> Collections { get; set; } = new();
    public List<ReleaseDetails> Releases { get; set; } = new();
    public List<NFTDetails> NFTs { get; set; } = new();
    public List<Nft1155Details> NFT1155s { get; set; } = new();
    public List<ReleaseSaleDetails> ReleaseSales { get; set; } = new();
    public List<ReleaseSaleDetails> Release1155Sales { get; set; } = new();
    public List<FactoryNftList> FactoryNftLists { get; set; } = new();
    public List<ListingDetails1155> Listings1155 { get; set; } = new();
    public List<Release1155Details> Releases1155 { get; set; } = new();

    public bool ListingsUpdated;
    public bool Listings1155Updated;
    public bool CollectionsUpdated;
    public bool ReleasesUpdated;
    public bool Releases1155Updated;
    public bool TokensUpdated;
    public bool SaleCollectionsUpdated;
    public bool ReleaseSalesUpdated;
    public bool ReleaseSales1155Updated;
    public bool ListingCollectionsUpdated;
    public bool IPFSUpdated;
    public bool NFTAPIUpdated;

    public int LastSyncTime;
    public bool LastSyncComplete;
    
    public void ClearStatus() {
        ListingsUpdated = false;
        Listings1155Updated = false;
        CollectionsUpdated = false;
        ReleasesUpdated = false;
        Releases1155Updated = false;
        TokensUpdated = false;
        SaleCollectionsUpdated = false;
        ReleaseSalesUpdated = false;
        ReleaseSales1155Updated = false;
        ListingCollectionsUpdated = false;
        IPFSUpdated = false;
        NFTAPIUpdated = false;
        LastSyncComplete = false;
    }

    public void AddStatus(StringBuilder builder) {
        builder.AppendLine($"Sync Status For Chain {ChainID}");
        builder.AppendLine("=======================================");
        builder.AppendLine($"ERC721 Listings Updated       : {ListingsUpdated}");
        builder.AppendLine($"ERC1155 Listings Updated      : {Listings1155Updated}");
        builder.AppendLine($"Collections Updated           : {CollectionsUpdated}");
        builder.AppendLine($"ERC721 Releases Updated       : {ReleasesUpdated}");
        builder.AppendLine($"ERC1155 Releases Updated      : {Releases1155Updated}");
        builder.AppendLine($"Tokens Updated                : {TokensUpdated}");
        builder.AppendLine($"Sales Collections Updated     : {SaleCollectionsUpdated}");
        builder.AppendLine($"ERC721 Release Sales Updated  : {ReleaseSalesUpdated}");
        builder.AppendLine($"ERC1155 Release Sales Updated : {ReleaseSales1155Updated}");
        builder.AppendLine($"Listing Collections Updated   : {ListingCollectionsUpdated}");
        builder.AppendLine($"IPFS Updated                  : {IPFSUpdated}");
        builder.AppendLine($"NFT API Updated               : {NFTAPIUpdated}");
        builder.AppendLine($"Last Release Scanned Block    : {LastReleaseScannedBlock}");
        builder.AppendLine($"Last Sync Time (seconds)      : {LastSyncTime}");
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
        Release1155StartingBlock = 21867453;
    }
}

public class NervosTestnetDefaults : OblivionDetails {
    public NervosTestnetDefaults() {
        ChainID = ChainID.Nervos_Testnet;
        ReleaseStartingBlock = 87064;
    }
}

public class NervosMainnetDefaults : OblivionDetails {
    public NervosMainnetDefaults() {
        ChainID = ChainID.Nervos_Mainnet;
        ReleaseStartingBlock = 25791;
    }
}

public class MaticTestnetDefaults : OblivionDetails {
    public MaticTestnetDefaults() {
        ChainID = ChainID.Matic_Testnet;
        ReleaseStartingBlock = 1658432363;
    }
}

public class MaticMainnetDefaults : OblivionDetails {
    public MaticMainnetDefaults() {
        ChainID = ChainID.Matic_Mainnet;
        ReleaseStartingBlock = 1658510444;
    }
}