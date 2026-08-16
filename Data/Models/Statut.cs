using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class Statut
{
    public int IdStatut { get; set; }

    public string Libelle { get; set; } = null!;

    public bool EstDefaut { get; set; }

    public virtual ICollection<HistoriqueStatut> HistoriqueStatutIdStatutPrecedentNavigations { get; set; } = new List<HistoriqueStatut>();

    public virtual ICollection<HistoriqueStatut> HistoriqueStatutIdStatutSuivantNavigations { get; set; } = new List<HistoriqueStatut>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Statut> IdStatutDestinations { get; set; } = new List<Statut>();

    public virtual ICollection<Statut> IdStatutOrigines { get; set; } = new List<Statut>();
}
