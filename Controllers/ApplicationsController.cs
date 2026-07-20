using Microsoft.AspNetCore.Mvc;
using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    public ApplicationsController(IApplicationService applicationService) => _applicationService = applicationService;

    [HttpPost]
    public async Task<IActionResult> Create(CreateApplicationRequest req)
    {
        var created = await _applicationService.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.IdApplication }, ToResponse(created));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _applicationService.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(ToResponse(item));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _applicationService.GetAllAsync();
        return Ok(items.Select(ToResponse));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateApplicationRequest req)
    {
        var ok = await _applicationService.UpdateAsync(id, req);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _applicationService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    private static ApplicationResponse ToResponse(Application a) => new(
        a.IdApplication,
        a.Libelle,
        a.Actif,
        a.IdCs);
}
