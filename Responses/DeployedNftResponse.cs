using Nethereum.ABI.FunctionEncoding.Attributes;

namespace OblivionAPI.Responses; 

[FunctionOutput]
public class DeployedNftResponse : IFunctionOutputDTO {
    [Parameter("address", "", 1)]
    public string Nft { get; set; }
    [Parameter("address", "", 2)]
    public string Deployer { get; set; }
    [Parameter("uint", "", 3)]
    public uint Created { get; set; }
}
