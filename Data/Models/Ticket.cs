using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class Ticket
{
    public int IdTicket { get; set; }

    public string NumeroTicket { get; set; } = null!;

    public DateTime DateCreation { get; set; }

    public int? IdApplication { get; set; }

    public int? IdTypeDemande { get; set; }

    public int? IdCriticite { get; set; }

    public int IdStatut { get; set; }

    public int? IdTechnicienAssigne { get; set; }

    public string DemandeurEmail { get; set; } = null!;

    public string DemandeurDirection { get; set; } = null!;

    public DateTime? DateCloture { get; set; }

    public decimal DureeSla { get; set; }

    public virtual ICollection<Commentaire> Commentaires { get; set; } = new List<Commentaire>();

    public virtual ICollection<EmailsSource> EmailsSources { get; set; } = new List<EmailsSource>();

    public virtual ICollection<Escalade> Escalades { get; set; } = new List<Escalade>();

    public virtual ICollection<HistoriqueStatut> HistoriqueStatuts { get; set; } = new List<HistoriqueStatut>();

    public virtual Application? IdApplicationNavigation { get; set; }

    public virtual Criticite? IdCriticiteNavigation { get; set; }

    public virtual Statut IdStatutNavigation { get; set; } = null!;

    public virtual Utilisateur? IdTechnicienAssigneNavigation { get; set; }

    public virtual TypesDemande? IdTypeDemandeNavigation { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Reassignation> Reassignations { get; set; } = new List<Reassignation>();

    public virtual Rejet? Rejet { get; set; }
}
