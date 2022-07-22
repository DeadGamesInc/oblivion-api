/*
 *  OblivionAPI :: ContractDetails
 *
 *  This class is used to store the contract address mappings for each blockchain.
 * 
 */

namespace OblivionAPI.Objects; 

public class ContractDetails {
    private readonly string BSC_Testnet;
    private readonly string BSC_Mainnet;
    private readonly string Nervos_Testnet;
    private readonly string Nervos_Mainnet;
    private readonly string Matic_Testnet;
    private readonly string Matic_Mainnet;

    public ContractDetails(string bscMainnet, string bscTestnet, string nervosTestnet, string nervosMainnet, string maticTestnet, string maticMainnet) {
        BSC_Mainnet = bscMainnet;
        BSC_Testnet = bscTestnet;
        Nervos_Testnet = nervosTestnet;
        Nervos_Mainnet = nervosMainnet;
        Matic_Testnet = maticTestnet;
        Matic_Mainnet = maticMainnet;
    }

    public string GetAddress(ChainID chainID) => chainID switch {
        ChainID.BSC_Mainnet => BSC_Mainnet,
        ChainID.BSC_Testnet => BSC_Testnet,
        ChainID.Nervos_Testnet => Nervos_Testnet,
        ChainID.Nervos_Mainnet => Nervos_Mainnet,
        ChainID.Matic_Testnet => Matic_Testnet,
        ChainID.Matic_Mainnet => Matic_Mainnet,
        _ => ""
    };
}