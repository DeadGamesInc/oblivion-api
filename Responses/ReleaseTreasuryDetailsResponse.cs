/*
 *  OblivionAPI :: ReleaseTreasuryDetailsResponse
 *
 *  This class defines the field mappings for the smart contract release treasury details response.
 * 
 */

using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Collections.Generic;

namespace OblivionAPI.Responses; 

[FunctionOutput]
public class ReleaseTreasuryDetailsResponse : IFunctionOutputDTO {
    [Parameter("address[]", "", 1)]
    public List<string> TreasuryAddresses { get; set; }
    [Parameter("uint[]", "", 2)]
    public List<uint> TreasuryAllocations { get; set; }
}