/*
 *  OblivionAPI :: OfferResponse
 *
 *  This class defines the field mappings for the smart contract offer response.
 * 
 */

using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace OblivionAPI.Responses; 

[FunctionOutput]
public class OfferResponse : IFunctionOutputDTO {
    [Parameter("address", "", 1)]
    public string Offeror { get; set; }
    [Parameter("uint", "", 2)]
    public BigInteger Amount { get; set; }
    [Parameter("uint", "", 3)]
    public uint Discount { get; set; }
    [Parameter("bool", "", 4)]
    public bool Claimed { get; set; }
    [Parameter("uint", "", 5)]
    public uint CreateBlock { get; set; }
    [Parameter("uint", "", 6)]
    public uint EndBlock { get; set; }
}