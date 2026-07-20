using Microsoft.AspNetCore.Mvc;
using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/criticites")]
public class CriticitesController : ControllerBase
{
    private readonly ICriticiteService _criticiteService;
    public CriticitesController(ICriticiteService criticiteService) => _criticiteService = criticiteService;

    [HttpPost]
    public async Task<IActionResult> Create(CreateCriticiteRequest req)
    {
        var created = await _criticiteService.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.IdCriticite }, ToResponse(created));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _criticiteService.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(ToResponse(item));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _criticiteService.GetAllAsync();
        return Ok(items.Select(ToResponse));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCriticiteRequest req)
    {
        var ok = await _criticiteService.UpdateAsync(id, req);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _criticiteService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    private static CriticiteResponse ToResponse(Criticite c) => new(
        c.IdCriticite,
        c.Libelle,
        c.Ordre);
}
