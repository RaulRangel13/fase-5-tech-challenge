using AGRO.Ingestion.Service.Application.DTOs;
using AGRO.Ingestion.Service.Infrastructure.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace AGRO.Ingestion.Service.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly RabbitMqPublisher _publisher;

    public TelemetryController(RabbitMqPublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    public IActionResult Post([FromBody] TelemetryData data)
    {
        try
        {
            _publisher.Publish(data);
            return Accepted(new { message = "Data queued for processing" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error publishing data", error = ex.Message });
        }
    }
}
