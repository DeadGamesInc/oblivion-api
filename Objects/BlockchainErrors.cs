namespace OblivionAPI.Objects; 

public abstract class BlockchainErrors {
    public ChainID ChainID;
    public int Timeouts;
    public int ContractErrors;
    public int Exceptions;
}

public class BSCMainnetErrors : BlockchainErrors { public BSCMainnetErrors() { ChainID = ChainID.BSC_Mainnet; } }
public class BSCTestnetErrors : BlockchainErrors { public BSCTestnetErrors() { ChainID = ChainID.BSC_Testnet; } }
public class NervosTestnetErrors : BlockchainErrors { public NervosTestnetErrors() { ChainID = ChainID.Nervos_Testnet; } }
