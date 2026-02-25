namespace AGRO.Alert.Service.Domain.Entities;

public class SensorReading
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FieldId { get; set; }
    public DateTime Timestamp { get; set; }
    public double SoilMoisture { get; set; }  // %
    public double Temperature { get; set; }   // Celsius
    public double Rainfall { get; set; }      // mm
}
