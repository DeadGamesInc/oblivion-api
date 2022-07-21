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

    public ContractDetails(string bscMainnet, string bscTestnet, string nervosTestnet, string nervosMainnet, string maticTestnet) {
        BSC_Mainnet = bscMainnet;
        BSC_Testnet = bscTestnet;
        Nervos_Testnet = nervosTestnet;
        Nervos_Mainnet = nervosMainnet;
        Matic_Testnet = maticTestnet;
    }

    public string GetAddress(ChainID chainID) => chainID switch {
        ChainID.BSC_Mainnet => BSC_Mainnet,
        ChainID.BSC_Testnet => BSC_Testnet,
        ChainID.Nervos_Testnet => Nervos_Testnet,
        ChainID.Nervos_Mainnet => Nervos_Mainnet,
        ChainID.Matic_Testnet => Matic_Testnet,
        _ => ""
    };
}