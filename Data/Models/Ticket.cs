using System;
using System.Collections.Generic;
using Pgvector;

namespace SIGRA.Data.Models;

public partial class Ticket
{
    public int IdTicket { get; set; }

    public string NumeroTicket { get; set; } = null!;

    public DateTime DateCreation { get; set; }

    public int? IdApplication { get; set; }

    public int? IdCriticite { get; set; }

    public int IdStatut { get; set; }

    public int? IdTechnicienAssigne { get; set; }

    public string DemandeurEmail { get; set; } = null!;

    public string DemandeurDirection { get; set; } = null!;

    public DateTime? DateCloture { get; set; }

    public decimal DureeSla { get; set; }

    public DateTime? DeadlineResolution { get; set; }

    public DateTime? DateChangementStatut { get; set; }

    public string CauseRacineIdentifie { get; set; } = null!;

    public bool ExclureConnaissancesIa { get; set; } = true;

    public Vector? DescriptionEmbedding { get; set; }

    public int? NombreRecurrence { get; set; }

    public int? IdTicketLieMemeCas { get; set; }

    public virtual ICollection<AlerteTicket> AlerteTickets { get; set; } = new List<AlerteTicket>();

    public virtual ICollection<Commentaire> Commentaires { get; set; } = new List<Commentaire>();

    public virtual ICollection<EmailsSource> EmailsSources { get; set; } = new List<EmailsSource>();

    public virtual ICollection<Escalade> Escalades { get; set; } = new List<Escalade>();

    public virtual ICollection<HistoriqueStatut> HistoriqueStatuts { get; set; } = new List<HistoriqueStatut>();

    public virtual Application? IdApplicationNavigation { get; set; }

    public virtual Criticite? IdCriticiteNavigation { get; set; }

    public virtual Statut IdStatutNavigation { get; set; } = null!;

    public virtual Utilisateur? IdTechnicienAssigneNavigation { get; set; }

    public virtual Ticket? IdTicketLieMemeCasNavigation { get; set; }

    public virtual ICollection<Ticket> InverseIdTicketLieMemeCasNavigation { get; set; } = new List<Ticket>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Reassignation> Reassignations { get; set; } = new List<Reassignation>();

    public virtual Rejet? Rejet { get; set; }

    public virtual ICollection<TicketSlaPause> TicketSlaPauses { get; set; } = new List<TicketSlaPause>();
}
