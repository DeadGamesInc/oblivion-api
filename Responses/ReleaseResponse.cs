/*
 *  OblivionAPI :: CollectionNFTsResponse
 *
 *  This class defines the field mappings for the smart contract release response.
 * 
 */

using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace OblivionAPI.Responses; 

[FunctionOutput]
public class ReleaseResponse : IFunctionOutputDTO {
    [Parameter("address", "", 1)]
    public string Owner { get; set; }
    [Parameter("address", "", 2)]
    public string NFT { get; set; }
    [Parameter("address", "", 3)]
    public string PaymentToken { get; set; }
    [Parameter("uint", "", 4)]
    public BigInteger Price { get; set; }
    [Parameter("uint", "", 5)]
    public uint Sales { get; set; }
    [Parameter("uint", "", 6)]
    public uint MaxSales { get; set; }
    [Parameter("uint", "", 7)]
    public uint EndDate { get; set; }
    [Parameter("uint", "", 8)]
    public uint MaxQuantity { get; set; }
    [Parameter("uint", "", 9)]
    public uint Discount { get; set; }
    [Parameter("bool", "", 10)]
    public bool Selectable { get; set; }
    [Parameter("bool", "", 11)]
    public bool Whitelisted { get; set; }
    [Parameter("bool", "", 12)]
    public bool Ended { get; set; }
    [Parameter("bool", "", 13)]
    public bool UsesReviveRug { get; set; }
}