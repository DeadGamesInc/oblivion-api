/*
 *  OblivionAPI :: BSCTestnetController
 *
 *  This controller inherits from the main OblivionController to set blockchain specific details.
 *  This controller should only contain the constructor to pass the details to the abstract controller, and set
 *  the class decorations.
 * 
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OblivionAPI.Objects;
using OblivionAPI.Services;

namespace OblivionAPI.Controllers; 

[ApiController]
[Route("/bsc_testnet/")]
public class BSCTestnetController : OblivionController {
    public BSCTestnetController(ILogger<BSCTestnetController> logger, DatabaseService database, ReportsService reports) 
        : base(logger, database, reports, ChainID.BSC_Testnet) {}
}