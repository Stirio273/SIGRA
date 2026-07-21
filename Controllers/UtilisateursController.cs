using Microsoft.AspNetCore.Mvc;
using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/utilisateurs")]
public class UtilisateursController : ControllerBase
{
    private readonly IUtilisateurService _utilisateurService;
    public UtilisateursController(IUtilisateurService utilisateurService) => _utilisateurService = utilisateurService;

    [HttpPost]
    public async Task<IActionResult> Create(CreateUtilisateurRequest req)
    {
        var created = await _utilisateurService.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.IdUtilisateur }, ToResponse(created));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _utilisateurService.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(ToResponse(item));
    }

    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var item = await _utilisateurService.GetByEmailAsync(email);
        return item is null ? NotFound() : Ok(ToResponse(item));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _utilisateurService.GetAllAsync();
        return Ok(items.Select(ToResponse));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUtilisateurRequest req)
    {
        var ok = await _utilisateurService.UpdateAsync(id, req);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _utilisateurService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    private static UtilisateurResponse ToResponse(Utilisateur u) => new(
        u.IdUtilisateur,
        u.IdentifiantAd,
        u.Nom,
        u.Prenom,
        u.Email,
        u.Actif,
        u.DateDesactivation,
        u.DateSynchronisation,
        u.IdRole,
        u.UserGuid);
}
