using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class Utilisateur
{
    public int IdUtilisateur { get; set; }

    public string IdentifiantAd { get; set; } = null!;

    public string Nom { get; set; } = null!;

    public string Prenom { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool Actif { get; set; }

    public DateTime? DateDesactivation { get; set; }

    public DateTime DateSynchronisation { get; set; }

    public int IdRole { get; set; }

    public Guid UserGuid { get; set; }

    public virtual ICollection<Commentaire> Commentaires { get; set; } = new List<Commentaire>();

    public virtual ICollection<Escalade> Escalades { get; set; } = new List<Escalade>();

    public virtual ICollection<HistoriqueStatut> HistoriqueStatuts { get; set; } = new List<HistoriqueStatut>();

    public virtual Role IdRoleNavigation { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Reassignation> ReassignationIdAncienAssigneNavigations { get; set; } = new List<Reassignation>();

    public virtual ICollection<Reassignation> ReassignationIdAuteurNavigations { get; set; } = new List<Reassignation>();

    public virtual ICollection<Reassignation> ReassignationIdNouvelAssigneNavigations { get; set; } = new List<Reassignation>();

    public virtual ICollection<Rejet> RejetIdAuteurNavigations { get; set; } = new List<Rejet>();

    public virtual ICollection<Rejet> RejetIdValidateurNavigations { get; set; } = new List<Rejet>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
