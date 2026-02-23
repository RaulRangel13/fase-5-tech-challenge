using AGRO.Management.Service.Application.DTOs;
using AGRO.Management.Service.Domain.Entities;
using AGRO.Management.Service.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AGRO.Management.Service.Application.Services;

public class FarmService
{
    private readonly ManagementDbContext _context;

    public FarmService(ManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<FarmDto>> GetAllFarmsAsync()
    {
        var farms = await _context.Farms.Include(f => f.Fields).ToListAsync();
        return farms.Select(f => new FarmDto(
            f.Id, 
            f.Name, 
            f.Location, 
            f.Fields.Select(fi => new FieldDto(fi.Id, fi.Name, fi.AreaHectares, fi.CropType, fi.FarmId)).ToList()
        )).ToList();
    }

    public async Task<FarmDto> CreateFarmAsync(CreateFarmDto dto)
    {
        var farm = new Farm { Name = dto.Name, Location = dto.Location };
        _context.Farms.Add(farm);
        await _context.SaveChangesAsync();

        return new FarmDto(farm.Id, farm.Name, farm.Location, new List<FieldDto>());
    }

    public async Task<FieldDto?> CreateFieldAsync(Guid farmId, CreateFieldDto dto)
    {
        var farm = await _context.Farms.FindAsync(farmId);
        if (farm == null) return null;

        var field = new Field
        {
            Name = dto.Name,
            AreaHectares = dto.AreaHectares,
            CropType = dto.CropType,
            FarmId = farmId
        };

        _context.Fields.Add(field);
        await _context.SaveChangesAsync();

        return new FieldDto(field.Id, field.Name, field.AreaHectares, field.CropType, field.FarmId);
    }
}
