using System.ComponentModel.DataAnnotations;

namespace AGRO.Identity.Service.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Producer"; // Producer, Admin

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
