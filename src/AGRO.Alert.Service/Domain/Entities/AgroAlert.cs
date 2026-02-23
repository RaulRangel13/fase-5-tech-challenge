using System.ComponentModel.DataAnnotations;

namespace AGRO.Alert.Service.Domain.Entities;

public class AgroAlert
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid FieldId { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;

    public string Severity { get; set; } = "Info"; // Info, Warning, Critical

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public double TriggerValue { get; set; } // O valor que disparou (ex: 15% umidade)
}
