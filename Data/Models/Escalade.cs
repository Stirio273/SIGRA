using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class Escalade
{
    public int IdEscalade { get; set; }

    public int IdTicket { get; set; }

    public int IdEntiteExterne { get; set; }

    public int IdAuteur { get; set; }

    public DateTime DateEscalade { get; set; }

    public string Explication { get; set; } = null!;

    public bool EstDefinitif { get; set; }

    public virtual Utilisateur IdAuteurNavigation { get; set; } = null!;

    public virtual EntitesExterne IdEntiteExterneNavigation { get; set; } = null!;

    public virtual Ticket IdTicketNavigation { get; set; } = null!;
}
