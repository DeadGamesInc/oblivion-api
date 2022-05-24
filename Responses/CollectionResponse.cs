/*
 *  OblivionAPI :: CollectionResponse
 *
 *  This class defines the field mappings for the smart contract collection response.
 * 
 */

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace OblivionAPI.Responses; 

[FunctionOutput]
public class CollectionResponse : IFunctionOutputDTO {
    [Parameter("address", "", 1)]
    public string Owner { get; set; }
    [Parameter("address", "", 2)]
    public string Treasury { get; set; }
    [Parameter("uint", "", 3)]
    public uint Royalties { get; set; }
    [Parameter("uint", "", 4)]
    public uint CreateBlock { get; set; }
}