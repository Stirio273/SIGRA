using SIGRA.Data.Models;

namespace SIGRA.Domain;

public interface ITicketContextMapper
{
    TicketContext Map(Ticket ticket);
}

public class TicketContextMapper : ITicketContextMapper
{
    public TicketContext Map(Ticket ticket)
    {
        return new TicketContext
        {
            IdTicket = ticket.IdTicket,
            Title = "",
            Description = "",
            Application = "",
            Category = "",
            Priority = "",
            Status = "",
            CreatedAt = ticket.DateCreation
        };
    }
}
