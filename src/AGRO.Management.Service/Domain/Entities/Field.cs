using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AGRO.Management.Service.Domain.Entities;

public class Field
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = string.Empty;

    public double AreaHectares { get; set; }

    public string CropType { get; set; } = "Generic"; // Soja, Milho, etc.

    public Guid FarmId { get; set; }
    
    [JsonIgnore]
    public Farm? Farm { get; set; }
}
