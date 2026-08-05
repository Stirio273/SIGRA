using SIGRA.Data.Models;

namespace SIGRA.Services;

public interface ITicketSlaService
{
    Task<DateTime> CalculateSlaAsync(Ticket ticket);
}