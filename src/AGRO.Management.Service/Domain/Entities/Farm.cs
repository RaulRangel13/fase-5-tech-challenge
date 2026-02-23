using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AGRO.Management.Service.Domain.Entities;

public class Farm
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    // Navigation Property
    [JsonIgnore]
    public List<Field> Fields { get; set; } = new();
}
