using AGRO.Alert.Service.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGRO.Alert.Service.Controllers;

[ApiController]
[Route("api/status")]
public class StatusController : ControllerBase
{
    private readonly AlertDbContext _db;

    public StatusController(AlertDbContext db)
    {
        _db = db;
    }

    [HttpGet("field/{fieldId}")]
    public async Task<IActionResult> GetFieldStatus(Guid fieldId)
    {
        var now = DateTime.UtcNow;
        var last24h = now.AddHours(-24);
        var last6h = now.AddHours(-6);

        // Leituras recentes
        var recentReadings = await _db.SensorReadings
            .Where(r => r.FieldId == fieldId && r.Timestamp >= last24h)
            .OrderByDescending(r => r.Timestamp)
            .Take(100)
            .ToListAsync();

        // Alertas recentes (últimas 48h)
        var recentAlerts = await _db.Alerts
            .Where(a => a.FieldId == fieldId && a.Timestamp >= now.AddHours(-48))
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        var status = "Normal";
        var message = "Condições normais.";

        // Verificar Risco de Praga (alerta de temperatura recente)
        var pestAlert = recentAlerts.FirstOrDefault(a => a.Message.Contains("Praga"));
        if (pestAlert != null)
        {
            status = "Risco de Praga";
            message = pestAlert.Message;
        }
        // Verificar Alerta de Seca (umidade < 30% por 24h)
        else
        {
            var droughtAlert = recentAlerts.FirstOrDefault(a => a.Message.Contains("seca"));
            if (droughtAlert != null)
            {
                status = "Alerta de Seca";
                message = droughtAlert.Message;
            }
            // Avaliar por leituras: umidade baixa por 24h?
            else if (recentReadings.Count >= 2)
            {
                var oldest = recentReadings.Min(r => r.Timestamp);
                var spanHours = (recentReadings.Max(r => r.Timestamp) - oldest).TotalHours;
                var allBelow30 = recentReadings.All(r => r.SoilMoisture < 30);
                if (spanHours >= 24 && allBelow30)
                {
                    status = "Alerta de Seca";
                    message = "Umidade do solo abaixo de 30% por mais de 24 horas.";
                }
                // Temperatura alta
                else if (recentReadings.Count(r => r.Timestamp >= last6h && r.Temperature > 38) >= 2)
                {
                    status = "Risco de Praga";
                    message = "Temperatura elevada detectada.";
                }
            }
        }

        return Ok(new { fieldId, status, message, updatedAt = now });
    }
}
