namespace OblivionAPI.Objects; 

public class ContractDetailsArray {
    private readonly string[] BSC_Testnet;
    private readonly string[] BSC_Mainnet;
    private readonly string[] Nervos_Testnet;
    private readonly string[] Nervos_Mainnet;

    public ContractDetailsArray(string[] bscMainnet, string[] bscTestnet, string[] nervosTestnet, string[] nervosMainnet) {
        BSC_Mainnet = bscMainnet;
        BSC_Testnet = bscTestnet;
        Nervos_Testnet = nervosTestnet;
        Nervos_Mainnet = nervosMainnet;
    }

    public string[] GetAddresses(ChainID chainID) => chainID switch {
        ChainID.BSC_Mainnet => BSC_Mainnet,
        ChainID.BSC_Testnet => BSC_Testnet,
        ChainID.Nervos_Testnet => Nervos_Testnet,
        ChainID.Nervos_Mainnet => Nervos_Mainnet,
        _ => null
    };
}
