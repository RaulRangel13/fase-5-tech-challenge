using AGRO.Management.Service.Application.DTOs;
using AGRO.Management.Service.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGRO.Management.Service.Controllers;

[ApiController]
[Route("api/farms")]
[Authorize] // Enforce JWT Auth
public class FarmsController : ControllerBase
{
    private readonly FarmService _service;

    public FarmsController(FarmService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllFarmsAsync());
    }

    [HttpPost]
    public async Task<IActionResult> CreateFarm([FromBody] CreateFarmDto dto)
    {
        var result = await _service.CreateFarmAsync(dto);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpPost("{farmId}/fields")]
    public async Task<IActionResult> CreateField(Guid farmId, [FromBody] CreateFieldDto dto)
    {
        var result = await _service.CreateFieldAsync(farmId, dto);
        if (result == null) return NotFound("Farm not found");
        return Ok(result);
    }
}
