namespace OblivionAPI.Objects; 

public abstract class BlockchainStats {
    public ChainID ChainID;
    public int Operations;
    public int AverageOperationTime;
    public int TotalOperationTime;

    public void AddOperation(long totalTime) {
        Operations++;
        TotalOperationTime += (int) totalTime;
        AverageOperationTime = TotalOperationTime / Operations;
    }
}

public class BSCMainnetStats : BlockchainStats { public BSCMainnetStats() { ChainID = ChainID.BSC_Mainnet; } }
public class BSCTestnetStats : BlockchainStats { public BSCTestnetStats() { ChainID = ChainID.BSC_Testnet; } }
public class NervosTestnetStats : BlockchainStats { public NervosTestnetStats() { ChainID = ChainID.Nervos_Testnet; } }
public class NervosMainnetStats : BlockchainStats { public NervosMainnetStats() { ChainID = ChainID.Nervos_Mainnet; } }