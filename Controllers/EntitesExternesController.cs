using Microsoft.AspNetCore.Mvc;
using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/entites-externes")]
public class EntitesExternesController : ControllerBase
{
    private readonly IEntitesExterneService _entitesExterneService;
    public EntitesExternesController(IEntitesExterneService entitesExterneService) => _entitesExterneService = entitesExterneService;

    [HttpPost]
    public async Task<IActionResult> Create(CreateEntitesExterneRequest req)
    {
        var created = await _entitesExterneService.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.IdEntiteExterne }, ToResponse(created));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _entitesExterneService.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(ToResponse(item));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _entitesExterneService.GetAllAsync();
        return Ok(items.Select(ToResponse));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateEntitesExterneRequest req)
    {
        var ok = await _entitesExterneService.UpdateAsync(id, req);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _entitesExterneService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    private static EntitesExterneResponse ToResponse(EntitesExterne e) => new(
        e.IdEntiteExterne,
        e.Nom,
        e.Actif);
}
