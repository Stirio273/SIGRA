using Microsoft.AspNetCore.Mvc;
using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    public RolesController(IRoleService roleService) => _roleService = roleService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _roleService.GetAllAsync();
        return Ok(items.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _roleService.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(ToResponse(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleRequest req)
    {
        var created = await _roleService.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.IdRole }, ToResponse(created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateRoleRequest req)
    {
        var ok = await _roleService.UpdateAsync(id, req);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _roleService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    private static RoleResponse ToResponse(Role r) => new(r.IdRole, r.Libelle);
}
