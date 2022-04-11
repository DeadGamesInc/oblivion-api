/*
 *  OblivionAPI :: ListingResponse
 *
 *  This class defines the field mappings for the smart contract listing response.
 * 
 */

using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace OblivionAPI.Responses {
    [FunctionOutput]
    public class ListingResponse : IFunctionOutputDTO {
        [Parameter("address", "", 1)]
        public string Owner { get; set; }
        [Parameter("address", "", 2)]
        public string PaymentToken { get; set; }
        [Parameter("address", "", 3)]
        public string NFT { get; set; }
        [Parameter("uint", "", 4)]
        public BigInteger TargetPrice { get; set; }
        [Parameter("uint", "", 5)]
        public BigInteger MinimumPrice { get; set; }
        [Parameter("uint", "", 6)]
        public uint TokenID { get; set; }
        [Parameter("uint", "", 7)]
        public uint SaleEnd { get; set; }
        [Parameter("uint", "", 8)]
        public uint GraceEnd { get; set; }
        [Parameter("uint", "", 9)]
        public uint CreateBlock { get; set; }
        [Parameter("uint", "", 10)]
        public uint ClosedBlock { get; set; }
        [Parameter("uint", "", 11)]
        public uint PaymentMethod { get; set; }
        [Parameter("uint", "", 12)]
        public uint SaleType { get; set; }
        [Parameter("uint", "", 13)]
        public uint SaleState { get; set; }
    }
}
