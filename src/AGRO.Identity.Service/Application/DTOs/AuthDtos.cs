using System.ComponentModel.DataAnnotations;

namespace AGRO.Identity.Service.Application.DTOs;

public record LoginDto([Required] string Email, [Required] string Password);

public record RegisterDto([Required, EmailAddress] string Email, [Required] string Password);

public record AuthResponse(string Token, string Email, string Role);
