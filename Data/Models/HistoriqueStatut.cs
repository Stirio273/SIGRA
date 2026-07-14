using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class HistoriqueStatut
{
    public int IdHistorique { get; set; }

    public int IdTicket { get; set; }

    public int? IdStatutPrecedent { get; set; }

    public int IdStatutSuivant { get; set; }

    public int IdAuteur { get; set; }

    public DateTime DateHeure { get; set; }

    public virtual Utilisateur IdAuteurNavigation { get; set; } = null!;

    public virtual Statut? IdStatutPrecedentNavigation { get; set; }

    public virtual Statut IdStatutSuivantNavigation { get; set; } = null!;

    public virtual Ticket IdTicketNavigation { get; set; } = null!;
}
