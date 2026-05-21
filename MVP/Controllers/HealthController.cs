using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVP.Data;

namespace MVP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db;

    public HealthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    // TODO: Éles környezetben [Authorize] attribútummal védeni,
    // vagy a válaszból elhagyni a database státuszt.
    public async Task<IActionResult> Get()
    {
        var databaseOk = await _db.Database.CanConnectAsync();

        return Ok(new
        {
            api = "ok",
            database = databaseOk ? "ok" : "not_connected",
            time = DateTime.UtcNow
        });
    }
}
