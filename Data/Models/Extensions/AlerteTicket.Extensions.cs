namespace SIGRA.Data.Models;

public partial class AlerteTicket
{
    private AlerteTicket() { }

    public static AlerteTicket Create(int ticketId, string alertType) => new()
    {
        IdTicket = ticketId,
        TypeAlerte = alertType,
        DateDeclenchement = DateTime.UtcNow
    };

    public void Expirer() => DateExpiration = DateTime.UtcNow;
}