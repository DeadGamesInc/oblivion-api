using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OblivionAPI.Controllers; 

[ApiController]
[Route("/nervos/")]
public class NervosMainnetController : OblivionController {
    public NervosMainnetController(ILogger<NervosMainnetController> logger, DatabaseService database, ReportsService reports) 
        : base(logger, database, reports, ChainID.Nervos_Mainnet) { }
}
