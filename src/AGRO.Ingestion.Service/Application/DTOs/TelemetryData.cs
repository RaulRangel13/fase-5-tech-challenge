using System.ComponentModel.DataAnnotations;

namespace AGRO.Ingestion.Service.Application.DTOs;

public class TelemetryData
{
    [Required]
    public Guid FieldId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public double SoilMoisture { get; set; } // %

    public double Temperature { get; set; } // Celsius

    public double Rainfall { get; set; } // mm
}
