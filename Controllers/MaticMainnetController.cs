using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OblivionAPI.Controllers; 

[ApiController]
[Route("/matic/")]
public class MaticMainnetController : OblivionController {
    public MaticMainnetController(ILogger<MaticMainnetController> logger, DatabaseService database, ReportsService reports) 
        : base(logger, database, reports, ChainID.Matic_Mainnet) {}
}
