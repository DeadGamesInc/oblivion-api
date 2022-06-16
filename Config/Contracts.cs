﻿/*
 *  OblivionAPI :: Contracts
 *
 *  This class stores the contract addresses for the market smart contracts.
 * 
 */

namespace OblivionAPI.Config; 

public static class Contracts {
    public static readonly ContractDetails OblivionMarket = new("0x1EAF08DD04cE415448a3d3F02abA83C85A4Fb0f0", "0x1D70Be618a8a85b46b52c89470a270C61ec1eCc6", "");
    public static readonly ContractDetails OblivionMarketV2 = new("0xCAD9E2AB8dC48bfd3779c2915421d834e7210726", "0x3Ed54FcdB2eea075F096166C2d48A5580Fd0F50D", "0xB9e43509dBa3aE5c7307dC887C5771744E5a4dC3");
    public static readonly ContractDetails OblivionCollectionManager = new("0x1A75487053676EDb05f9B9A8B73FE6f1580CfFd1", "0x47118A711D8aaAff53101DDD84F822E3f1D987B6", "0xD710425091cA0163CF73d8d51da0539b99DA981c");
    public static readonly ContractDetails OblivionMintingService = new("0xB1890952A926E5256Db3D5182a76cC1cF42ac5a6", "0x403EA5b2A7019f02664Db21C3de050a0bC85D62A", "0x393995149BAB398E24B0e1BEB90cc24245de883B");
}