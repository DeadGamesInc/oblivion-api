﻿/*
 *  OblivionAPI :: Contracts
 *
 *  This class stores the contract addresses for the market smart contracts.
 * 
 */

namespace OblivionAPI.Config; 

public static class Contracts {
    public static readonly ContractDetails OblivionMarket = new("0x1EAF08DD04cE415448a3d3F02abA83C85A4Fb0f0", "0x1D70Be618a8a85b46b52c89470a270C61ec1eCc6", "", "", "", "");
    public static readonly ContractDetails OblivionMarketV2 = new("0xCAD9E2AB8dC48bfd3779c2915421d834e7210726", "0x3Ed54FcdB2eea075F096166C2d48A5580Fd0F50D", "0xB9e43509dBa3aE5c7307dC887C5771744E5a4dC3", "0x06245A3ADA1B27793D4c9daedd6fba215243e41D", "0x4e5A00bc4aBf02efbaADad25AC3ea75eA645548a", "0x70f9Cf7E2CBFb604eF692386109758978eaA7A71");
    public static readonly ContractDetails OblivionCollectionManager = new("0x1A75487053676EDb05f9B9A8B73FE6f1580CfFd1", "0x47118A711D8aaAff53101DDD84F822E3f1D987B6", "0xD710425091cA0163CF73d8d51da0539b99DA981c", "0x56B8Af98281d4b00Ac3d7b2722459495677F16f3", "0xeBc5103EE33DC1Ea20B15fc81bD36158bEdc2B84", "0xFc71A4aA5484A46CbF502E0970f8b2719f6C7B91");
    public static readonly ContractDetails OblivionMintingService = new("0xB1890952A926E5256Db3D5182a76cC1cF42ac5a6", "0x403EA5b2A7019f02664Db21C3de050a0bC85D62A", "0x393995149BAB398E24B0e1BEB90cc24245de883B", "0xb060cA64f22cE53055960B73Af87fDF030DCd0ef", "0x32033306e092E404Ee9B206fBE044EA873219951", "0x3dCE8E07dD689E6C31686693A6817D323EAe8965");
    public static readonly ContractDetails OblivionMarket1155 = new("", "0x6709d83d06C5a593d064f0faeE1cE5Dbb831DA70", "", "", "", "");
    public static readonly ContractDetails OblivionMintingService1155 = new("0x53B7cDfbf3Aa964EfB170FcDe5d3165AA92FA5cc", "0x24ed90DAAe7C4F678AB766eDe8f8E7371287da97", "0xf3A3a31b90BE814A45170CbC9df0678219c03656", "0x387443f120e966f0455F15653dC3eDdF64579bAC", "0x17b75E897dd94dD0b5a1B1a65EaB67Db44379D0F", "0x10875F04A263dE9CAe5B63C1dDb66392374E8a02");

    public static readonly ContractDetailsArray NftFactories = 
        new(
            new[] { "" }, 
            new[] { "0x616f8590423BC7E4e621D092D8730486d7131bd4", "0x5Efb107c485F9f0d1fDfB6d4A298ab99477B709E", "0x3dF5dDb164d1b19b9AC589777365Eb76d1e9104f" }, 
            new[] { "0xE741be69CDe48EcD7B4EeF58C0E9C44E8B50165d", "0xb10fB1E0e838fDb9EFf57bB73da4a12B5D2b5A3e", "0x6BE597B6fa03A5A8F47485624c1ff4d7Bab6E94c" },
            new[] { "0xA2158da2534D2aF4fC2c5b902eeBE8Ed63fc25cc" },
            new[] { "0x8637822afF82e276E5d6dBC6b6bAA375cfA24D72", "0xa8207c25B06751f620aaf14f4FF4E0d7760da10B", "0x1E98da8AE967580A8f631484965cb8003dF3d0C0" },
            new[] { "0x32033306e092E404Ee9B206fBE044EA873219951", "0x06245A3ADA1B27793D4c9daedd6fba215243e41D", "0x8637822afF82e276E5d6dBC6b6bAA375cfA24D72" }
            );

    public static readonly ContractDetailsArray Nft1155Factories =
        new(
            new[] { "0xe723F8FaD2F9Ed6F7ec769a58474d140C0aD2d08" },
            new[] { "0xA7aD05cF47BC6dBde8664533c07397e84Eb03550" },
            new[] { "0x630D0127cABcc578163A6c5a3D23cF46D72421C8" },
            new[] { "0x0F567802D13E041704309E8b8940a4201136C20b" },
            new[] { "0xA6F5Fb773966ba25c1772412100C9B7fE6a9fABC" },
            new[] { "0x5Aa46df5cE00c37E9AEb4D1a8a9C476427176BC9" }
        );
}