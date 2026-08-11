using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class TicketSlaPause
{
    public int Id { get; set; }

    public int IdTicket { get; set; }

    public DateTime? PausedAt { get; set; }

    public DateTime? ResumedAt { get; set; }

    public virtual Ticket IdTicketNavigation { get; set; } = null!;
}
