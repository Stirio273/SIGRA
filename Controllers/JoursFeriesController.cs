using Microsoft.AspNetCore.Mvc;
using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/jours-feries")]
public class JoursFeriesController : ControllerBase
{
    private readonly IJoursFerieService _joursFerieService;
    public JoursFeriesController(IJoursFerieService joursFerieService) => _joursFerieService = joursFerieService;

    [HttpPost]
    public async Task<IActionResult> Create(CreateJoursFerieRequest req)
    {
        var created = await _joursFerieService.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.IdJourFerie }, ToResponse(created));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _joursFerieService.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(ToResponse(item));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _joursFerieService.GetAllAsync();
        return Ok(items.Select(ToResponse));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateJoursFerieRequest req)
    {
        var ok = await _joursFerieService.UpdateAsync(id, req);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _joursFerieService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    private static JoursFerieResponse ToResponse(JoursFerie j) => new(
        j.IdJourFerie,
        j.Date,
        j.Libelle);
}
