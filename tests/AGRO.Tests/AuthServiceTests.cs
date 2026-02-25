using Xunit;
using AGRO.Identity.Service.Application.Services;
using AGRO.Identity.Service.Infrastructure.Data;
using AGRO.Identity.Service.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AGRO.Tests;

public class AuthServiceTests
{
    private static IdentityDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new IdentityDbContext(options);
    }

    private static TokenService GetTokenService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "super_secret_key_for_hackathon_agro_solutions_2026"
            })
            .Build();
        return new TokenService(config);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndReturnToken()
    {
        var context = GetInMemoryDbContext();
        var tokenService = GetTokenService();
        var authService = new AuthService(context, tokenService);
        var dto = new RegisterDto("produtor@agro.com", "Senha123!");

        var result = await authService.RegisterAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("produtor@agro.com", result.Email);
        Assert.Equal("Producer", result.Role);
        Assert.False(string.IsNullOrEmpty(result.Token));
        var userCount = await context.Users.CountAsync();
        Assert.Equal(1, userCount);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ShouldThrow()
    {
        var context = GetInMemoryDbContext();
        var tokenService = GetTokenService();
        var authService = new AuthService(context, tokenService);
        await authService.RegisterAsync(new RegisterDto("duplicado@agro.com", "Senha123!"));

        await Assert.ThrowsAsync<Exception>(() =>
            authService.RegisterAsync(new RegisterDto("duplicado@agro.com", "OutraSenha1!")));
    }

    [Fact]
    public async Task LoginAsync_AfterRegister_ShouldReturnToken()
    {
        var context = GetInMemoryDbContext();
        var tokenService = GetTokenService();
        var authService = new AuthService(context, tokenService);
        await authService.RegisterAsync(new RegisterDto("login@agro.com", "Senha123!"));

        var result = await authService.LoginAsync(new LoginDto("login@agro.com", "Senha123!"));

        Assert.NotNull(result);
        Assert.Equal("login@agro.com", result.Email);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldThrow()
    {
        var context = GetInMemoryDbContext();
        var tokenService = GetTokenService();
        var authService = new AuthService(context, tokenService);
        await authService.RegisterAsync(new RegisterDto("user@agro.com", "Senha123!"));

        await Assert.ThrowsAsync<Exception>(() =>
            authService.LoginAsync(new LoginDto("user@agro.com", "SenhaErrada!")));
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldThrow()
    {
        var context = GetInMemoryDbContext();
        var tokenService = GetTokenService();
        var authService = new AuthService(context, tokenService);

        await Assert.ThrowsAsync<Exception>(() =>
            authService.LoginAsync(new LoginDto("naoexiste@agro.com", "QualquerSenha1!")));
    }
}
