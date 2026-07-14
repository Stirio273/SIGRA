using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class Criticite
{
    public int IdCriticite { get; set; }

    public string Libelle { get; set; } = null!;

    public int Ordre { get; set; }

    public virtual ICollection<RegleCriticite> RegleCriticites { get; set; } = new List<RegleCriticite>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
