﻿namespace OblivionAPI.Objects; 

public abstract class BlockchainErrors {
    public ChainID ChainID;
    public int Timeouts;
    public int ContractErrors;
    public int Exceptions;
    
    public int PreviousTimeouts;
    public int PreviousContractErrors;
    public int PreviousExceptions;
    
    public int TotalTimeouts;
    public int TotalContractErrors;
    public int TotalExceptions;
}

public class BSCMainnetErrors : BlockchainErrors { public BSCMainnetErrors() { ChainID = ChainID.BSC_Mainnet; } }
public class BSCTestnetErrors : BlockchainErrors { public BSCTestnetErrors() { ChainID = ChainID.BSC_Testnet; } }
public class NervosTestnetErrors : BlockchainErrors { public NervosTestnetErrors() { ChainID = ChainID.Nervos_Testnet; } }
public class NervosMainnetErrors : BlockchainErrors { public NervosMainnetErrors() { ChainID = ChainID.Nervos_Mainnet; } }
public class MaticTestnetErrors : BlockchainErrors { public MaticTestnetErrors() { ChainID = ChainID.Matic_Testnet; } }
public class MaticMainnetErrors : BlockchainErrors { public MaticMainnetErrors() { ChainID = ChainID.Matic_Mainnet; } }
