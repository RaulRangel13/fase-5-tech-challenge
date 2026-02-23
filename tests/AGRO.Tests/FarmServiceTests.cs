using Xunit;
using Moq;
using AGRO.Management.Service.Application.Services;
using AGRO.Management.Service.Infrastructure.Data;
using AGRO.Management.Service.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AGRO.Tests;

public class FarmServiceTests
{
    private ManagementDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new ManagementDbContext(options);
        return context;
    }

    [Fact]
    public async Task CreateFarmAsync_ShouldReturnCreatedFarm()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new FarmService(context);
        var dto = new CreateFarmDto("Fazenda Esperança", "Minas Gerais");

        // Act
        var result = await service.CreateFarmAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Location, result.Location);
    }

    [Fact]
    public async Task CreateFieldAsync_WhenFarmExists_ShouldReturnCreatedField()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new FarmService(context);
        
        var farmDto = new CreateFarmDto("Fazenda Alvorada", "Mato Grosso");
        var farm = await service.CreateFarmAsync(farmDto);

        var fieldDto = new CreateFieldDto("Talhão 1", 50.5, "Soja");

        // Act
        var result = await service.CreateFieldAsync(farm.Id, fieldDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fieldDto.Name, result.Name);
        Assert.Equal(fieldDto.CropType, result.CropType);
        Assert.Equal(fieldDto.AreaHectares, result.AreaHectares);
    }

    [Fact]
    public async Task CreateFieldAsync_WhenFarmDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new FarmService(context);
        var fieldDto = new CreateFieldDto("Talhão Fantasma", 10, "Milho");

        // Act
        var result = await service.CreateFieldAsync(Guid.NewGuid(), fieldDto);

        // Assert
        Assert.Null(result);
    }
}
