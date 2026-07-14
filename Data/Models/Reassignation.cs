using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class Reassignation
{
    public int IdReassignation { get; set; }

    public int IdTicket { get; set; }

    public int? IdAncienAssigne { get; set; }

    public int IdNouvelAssigne { get; set; }

    public string Motif { get; set; } = null!;

    public int IdAuteur { get; set; }

    public DateTime DateReassignation { get; set; }

    public virtual Utilisateur? IdAncienAssigneNavigation { get; set; }

    public virtual Utilisateur IdAuteurNavigation { get; set; } = null!;

    public virtual Utilisateur IdNouvelAssigneNavigation { get; set; } = null!;

    public virtual Ticket IdTicketNavigation { get; set; } = null!;
}
