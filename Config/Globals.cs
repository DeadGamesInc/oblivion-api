/*
 *  OblivionAPI :: Globals
 *
 *  This class stores the general configuration values for the API.
 * 
 */

namespace OblivionAPI.Config; 

public static class Globals {
    public static string LISTEN_PORT = "5001";
    public static uint CACHE_TIME = 2;
    public static uint REFRESH_TIME = 300000;
    public static int THROTTLE_WAIT = 500;
    public static int COIN_GECKO_THROTTLE_WAIT = 1500;
    
    public const int MAX_SYNC_TIME = 900;
    public const int FACTORY_NFT_PIN_DAYS = 7;

    public static string IMAGE_CACHE_PREFIX = "https://api.oblivion.art/image-cache/";

    public const string IPFS_RAW_PREFIX = "ipfs://";
    public const string IPFS_HTTP_PREFIX = "http://ipfs-gateway.deadgames.io:8080//ipfs/";
    public const string IPFS_CID_POST_URL = "https://ipfs-pinning-service.deadgames.io/api/v1/pins/batch";

    private static readonly string BASE_DIR = AppDomain.CurrentDomain.BaseDirectory;
    public static readonly string WEB_ROOT = Path.Combine(BASE_DIR, "wwwroot");
    public static readonly string IMAGE_CACHE_DIR = Path.Combine(WEB_ROOT, "image-cache");
    public static readonly string DB_FILE = Path.Combine(WEB_ROOT, "database.json");
        
    public static readonly List<BlockchainDetails> Blockchains = new() {
        new() { ChainID = ChainID.BSC_Mainnet, Node = "https://bsc-dataseed.binance.org", NftApiUri = "https://nft-api.deadgames.io/bsc/trackNfts" },
        new() { ChainID = ChainID.BSC_Testnet, Node = "https://data-seed-prebsc-1-s1.binance.org:8545", NftApiUri = "https://nft-api.deadgames.io/bsc_testnet/trackNfts" },
        new() { ChainID = ChainID.Nervos_Testnet, Node = "https://godwoken-testnet-v1.ckbapp.dev", NftApiUri = "https://nft-api.deadgames.io/nervos_testnet/trackNfts" },
        new() { ChainID = ChainID.Nervos_Mainnet, Node = "https://v1.mainnet.godwoken.io/rpc", NftApiUri = "https://nft-api.deadgames.io/nervos/trackNfts" }
    };

    public static readonly List<PaymentDetails> Payments = new() {
        new() { ChainID = ChainID.BSC_Mainnet, PaymentTokens = new() {
            new() { Address = "0x0000000000000000000000000000000000000000", Symbol = "BNB", CoinGeckoKey = "binancecoin" },
            new() { Address = "0x50ba8BF9E34f0F83F96a340387d1d3888BA4B3b5", Symbol = "ZMBE", CoinGeckoKey = "rugzombie" },
            new() { Address = "0xe9e7cea3dedca5984780bafc599bd69add087d56", Symbol = "BUSD", CoinGeckoKey = "binance-usd" }
        }},
        new() { ChainID = ChainID.BSC_Testnet, PaymentTokens = new() {
            new() { Address = "0x0000000000000000000000000000000000000000", Symbol = "BNB", CoinGeckoKey = "binancecoin" },
            new() { Address = "0x4c99c06cd1ec75a303b24e9e89374ebd672189ad", Symbol = "ZMBE", CoinGeckoKey = "rugzombie" },
            new() { Address = "0xed24fc36d5ee211ea25a80239fb8c4cfd80f12ee", Symbol = "BUSD", CoinGeckoKey = "binance-usd" }
        }},
        new() { ChainID = ChainID.Nervos_Testnet, PaymentTokens = new() {
            new() { Address = "0x0000000000000000000000000000000000000000", Symbol = "CKB", CoinGeckoKey = "nervos-network" }
        }},
        new() { ChainID = ChainID.Nervos_Mainnet, PaymentTokens = new() {
            new() { Address = "0x0000000000000000000000000000000000000000", Symbol = "CKB", CoinGeckoKey = "nervos-network" }
        }}
    };
}