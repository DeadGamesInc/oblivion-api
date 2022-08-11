using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace OblivionAPI.Responses; 

[FunctionOutput]
public class Release1155Response : IFunctionOutputDTO {
    [Parameter("address", "", 1)]
    public string Owner { get; set; }
    [Parameter("address", "", 2)]
    public string NFT { get; set; }
    [Parameter("uint", "", 3)]
    public uint TokenId { get; set; }
    [Parameter("address", "", 4)]
    public string PaymentToken { get; set; }
    [Parameter("uint", "", 5)]
    public BigInteger Price { get; set; }
    [Parameter("uint", "", 6)]
    public uint Sales { get; set; }
    [Parameter("uint", "", 7)]
    public uint MaxSales { get; set; }
    [Parameter("uint", "", 8)]
    public uint EndDate { get; set; }
    [Parameter("uint", "", 9)]
    public uint MaxQuantity { get; set; }
    [Parameter("uint", "", 10)]
    public uint Discount { get; set; }
    [Parameter("bool", "", 11)]
    public bool Whitelisted { get; set; }
    [Parameter("bool", "", 12)]
    public bool Ended { get; set; }
}
