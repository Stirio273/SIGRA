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
    public TicketsController(ITicketService ticketService) => _ticketService = ticketService;

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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateTicketRequest req)
    {
        var ok = await _ticketService.UpdateAsync(id, req);
        return ok ? NoContent() : NotFound();
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
        t.IdApplication,
        // t.IdTypeDemande,
        t.IdCriticite,
        t.IdStatut,
        t.IdTechnicienAssigne,
        t.DemandeurEmail,
        t.DemandeurDirection,
        t.DateCloture,
        t.DureeSla);
}
