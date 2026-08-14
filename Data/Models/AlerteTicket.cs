using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class AlerteTicket
{
    public int Id { get; set; }

    public int IdTicket { get; set; }

    public string TypeAlerte { get; set; } = null!;

    public DateTime DateDeclenchement { get; set; }

    public DateTime? DateExpiration { get; set; }

    public virtual Ticket IdTicketNavigation { get; set; } = null!;
}
