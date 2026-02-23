using System.ComponentModel.DataAnnotations;

namespace AGRO.Management.Service.Application.DTOs;

public record CreateFarmDto([Required] string Name, string Location);
public record CreateFieldDto([Required] string Name, double AreaHectares, string CropType);

public record FarmDto(Guid Id, string Name, string Location, List<FieldDto> Fields);
public record FieldDto(Guid Id, string Name, double AreaHectares, string CropType, Guid FarmId);
