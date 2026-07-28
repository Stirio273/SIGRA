using Microsoft.AspNetCore.Mvc;
using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Services;
using System;
using System.Linq;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IUserAuthenticationService _userAuthenticationService;

    public TicketsController(ITicketService ticketService, IUserAuthenticationService userAuthenticationService)
    {
        _ticketService = ticketService;
        _userAuthenticationService = userAuthenticationService;
    }

    [HttpPost("{id}/transfer")]
    public async Task<IActionResult> Transfer(TransferTicketRequest request)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var currentUser = await _userAuthenticationService.GetAuthorizedUserAsync(username);
        if (currentUser == null)
            return Unauthorized();

        await _ticketService.TransferAsync(request.idTicket, request.idEntiteExterne, currentUser.IdUtilisateur, request.explication, request.estDefinitif);
        return Ok();
    }

    [HttpPost("request-deny/respond")]
    public async Task<IActionResult> RespondDenyRequest(RespondDenyRequest req)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var currentUser = await _userAuthenticationService.GetAuthorizedUserAsync(username);
        if (currentUser == null)
            return Unauthorized();

        await _ticketService.RespondRejectDemandAsync(req.IdTicket, req.rejetId, currentUser.IdUtilisateur, req.decision);
        return NoContent();
    }

    [HttpPost("request-deny")]
    public async Task<IActionResult> SendDenyRequest(CreateDenyRequest req)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var currentUser = await _userAuthenticationService.GetAuthorizedUserAsync(username);
        if (currentUser == null)
            return Unauthorized();

        await _ticketService.AskRejectAsync(req.IdTicket, currentUser.IdUtilisateur, req.Justificatif);
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketRequest req)
    {
        var created = await _ticketService.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.IdTicket }, ToResponse(created));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _ticketService.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(ToResponse(item));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest req)
    {
        var result = await _ticketService.GetPagedAsync(req.PageNumber, req.PageSize);
        return Ok(new PagedResult<TicketResponse>
        {
            Items = result.Items.Select(ToResponse).ToList(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        });
    }

    [HttpGet("technician/{technicianUserGuid:guid}")]
    public async Task<IActionResult> GetByTechnician(Guid technicianUserGuid)
    {
        var items = await _ticketService.GetByTechnicianAsync(technicianUserGuid);
        return Ok(items.Select(ToResponse));
    }

    [HttpPatch("assign")]
    public async Task<IActionResult> Assign(AssignTicketsRequest req)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var ok = await _ticketService.AssignAsync(req.TicketIds, req.UserGuid, username);
        return ok ? NoContent() : Forbid();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateTicketRequest req)
    {
        var ok = await _ticketService.UpdateAsync(id, req);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/next-statuts")]
    public async Task<IActionResult> GetNextStatuts(int id)
    {
        var statuts = await _ticketService.GetNextStatutsAsync(id);
        return Ok(statuts.Select(s => new StatutSuivantPossibleResponse(s.IdStatut, s.Libelle)));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _ticketService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    private static TicketResponse ToResponse(Ticket t) => new(
        t.IdTicket,
        t.NumeroTicket,
        t.DateCreation,
        t.IdApplicationNavigation is not null ? new ApplicationRefResponse(t.IdApplicationNavigation.IdApplication, t.IdApplicationNavigation.Libelle) : null,
        t.IdCriticiteNavigation is not null ? new CriticiteRefResponse(t.IdCriticiteNavigation.IdCriticite, t.IdCriticiteNavigation.Libelle) : null,
        new StatutRefResponse(t.IdStatutNavigation.IdStatut, t.IdStatutNavigation.Libelle),
        t.IdTechnicienAssigneNavigation is not null ? new TechnicienRefResponse(t.IdTechnicienAssigneNavigation.IdUtilisateur, t.IdTechnicienAssigneNavigation.Email) : null,
        t.DemandeurEmail,
        t.DemandeurDirection,
        t.DateCloture,
        t.DureeSla);
}
