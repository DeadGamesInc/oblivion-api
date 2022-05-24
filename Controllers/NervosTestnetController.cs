using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OblivionAPI.Controllers; 

[ApiController]
[Route("/nervos_testnet/")]
public class NervosTestnetController : OblivionController {
    public NervosTestnetController(ILogger<NervosTestnetController> logger, DatabaseService database, ReportsService reports) 
        : base(logger, database, reports, ChainID.Nervos_Testnet) {}
}