using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OblivionAPI.Controllers; 

[ApiController]
[Route("/matic_testnet/")]
public class MaticTestnetController : OblivionController {
    public MaticTestnetController(ILogger<MaticTestnetController> logger, DatabaseService database, ReportsService reports) 
        : base(logger, database, reports, ChainID.Matic_Testnet) {}
}
