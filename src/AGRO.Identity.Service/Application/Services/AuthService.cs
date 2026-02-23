using AGRO.Identity.Service.Application.DTOs;
using AGRO.Identity.Service.Domain.Entities;
using AGRO.Identity.Service.Infrastructure.Data;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace AGRO.Identity.Service.Application.Services;

public class AuthService
{
    private readonly IdentityDbContext _context;
    private readonly TokenService _tokenService;

    public AuthService(IdentityDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            throw new Exception("Email already registered.");

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "Producer"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);
        return new AuthResponse(token, user.Email, user.Role);
    }

    public async Task<AuthResponse> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new Exception("Invalid credentials.");

        var token = _tokenService.GenerateToken(user);
        return new AuthResponse(token, user.Email, user.Role);
    }
}
