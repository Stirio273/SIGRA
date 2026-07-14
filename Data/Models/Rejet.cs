using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class Rejet
{
    public int IdRejet { get; set; }

    public int IdTicket { get; set; }

    public int IdAuteur { get; set; }

    public string Justificatif { get; set; } = null!;

    public DateTime DateProposition { get; set; }

    public int? IdValidateur { get; set; }

    public bool? Decision { get; set; }

    public DateTime? DateDecision { get; set; }

    public virtual Utilisateur IdAuteurNavigation { get; set; } = null!;

    public virtual Ticket IdTicketNavigation { get; set; } = null!;

    public virtual Utilisateur? IdValidateurNavigation { get; set; }
}
