/*
 *  OblivionAPI :: ContractDetails
 *
 *  This class is used to store the contract address mappings for each blockchain.
 * 
 */

namespace OblivionAPI.Objects {
    public class ContractDetails {
        private readonly string BSC_Testnet;
        private readonly string BSC_Mainnet;

        public ContractDetails(string bscMainnet, string bscTestnet) {
            BSC_Mainnet = bscMainnet;
            BSC_Testnet = bscTestnet;
        }

        public string GetAddress(ChainID chainID) => chainID switch {
                ChainID.BSC_Mainnet => BSC_Mainnet,
                ChainID.BSC_Testnet => BSC_Testnet,
                _ => ""
            };
    }
}
