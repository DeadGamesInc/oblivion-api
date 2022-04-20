/*
 *  OblivionAPI :: Contracts
 *
 *  This class stores the contract addresses for the market smart contracts.
 * 
 */

using OblivionAPI.Objects;

namespace OblivionAPI.Config {
    public static class Contracts {
        public static readonly ContractDetails OblivionMarket = new("0x1EAF08DD04cE415448a3d3F02abA83C85A4Fb0f0", "0x1D70Be618a8a85b46b52c89470a270C61ec1eCc6");
        public static readonly ContractDetails OblivionMarketV2 = new("", "0x3Ed54FcdB2eea075F096166C2d48A5580Fd0F50D");
        public static readonly ContractDetails OblivionCollectionManager = new("0x0D41cF21728dB24b45b0A72B2EA62c7Cd78a51d5", "0x47118A711D8aaAff53101DDD84F822E3f1D987B6");
        public static readonly ContractDetails OblivionMintingService = new("0xB1890952A926E5256Db3D5182a76cC1cF42ac5a6", "0x403EA5b2A7019f02664Db21C3de050a0bC85D62A");
    }
}
