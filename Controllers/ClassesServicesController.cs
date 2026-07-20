using Microsoft.AspNetCore.Mvc;
using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/classes-services")]
public class ClassesServicesController : ControllerBase
{
    private readonly IClassesServiceService _classesServiceService;
    public ClassesServicesController(IClassesServiceService classesServiceService) => _classesServiceService = classesServiceService;

    [HttpPost]
    public async Task<IActionResult> Create(CreateClassesServiceRequest req)
    {
        var created = await _classesServiceService.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.IdCs }, ToResponse(created));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _classesServiceService.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(ToResponse(item));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _classesServiceService.GetAllAsync();
        return Ok(items.Select(ToResponse));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateClassesServiceRequest req)
    {
        var ok = await _classesServiceService.UpdateAsync(id, req);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _classesServiceService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    private static ClassesServiceResponse ToResponse(ClassesService c) => new(
        c.IdCs,
        c.Code,
        c.Libelle);
}
