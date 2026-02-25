using AGRO.Alert.Service.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGRO.Alert.Service.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly AlertDbContext _db;

    public TelemetryController(AlertDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetReadings(
        [FromQuery] Guid fieldId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 500)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
        var toDate = to ?? DateTime.UtcNow;

        var readings = await _db.SensorReadings
            .Where(r => r.FieldId == fieldId && r.Timestamp >= fromDate && r.Timestamp <= toDate)
            .OrderBy(r => r.Timestamp)
            .Take(limit)
            .Select(r => new
            {
                r.Id,
                r.FieldId,
                r.Timestamp,
                r.SoilMoisture,
                r.Temperature,
                r.Rainfall
            })
            .ToListAsync();

        return Ok(readings);
    }
}
