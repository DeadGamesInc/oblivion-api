/*
 *  OblivionAPI :: CollectionNFTsResponse
 *
 *  This class defines the field mappings for the smart contract collection NFTs response.
 * 
 */

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace OblivionAPI.Responses; 

[FunctionOutput]
public class CollectionNFTsResponse : IFunctionOutputDTO {
    [Parameter("address[]", "", 1)]
    public List<string> NFTs { get; set; }
}