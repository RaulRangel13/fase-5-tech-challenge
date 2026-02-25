using AGRO.Alert.Service.Domain.Entities;
using AGRO.Alert.Service.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGRO.Alert.Service.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController : ControllerBase
{
    private readonly AlertDbContext _db;

    public AlertsController(AlertDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAlerts([FromQuery] Guid? fieldId)
    {
        var query = _db.Alerts.AsQueryable();
        if (fieldId.HasValue)
            query = query.Where(a => a.FieldId == fieldId.Value);
        var alerts = await query.OrderByDescending(a => a.Timestamp).Take(100).ToListAsync();
        return Ok(alerts);
    }

    [HttpGet("field/{fieldId}")]
    public async Task<IActionResult> GetAlertsByField(Guid fieldId)
    {
        var alerts = await _db.Alerts
            .Where(a => a.FieldId == fieldId)
            .OrderByDescending(a => a.Timestamp)
            .Take(50)
            .ToListAsync();
        return Ok(alerts);
    }
}
