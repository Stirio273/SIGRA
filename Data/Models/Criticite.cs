using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class Criticite
{
    public int IdCriticite { get; set; }

    public string Libelle { get; set; } = null!;

    public int Ordre { get; set; }

    public virtual ICollection<ReglesCriticite> ReglesCriticites { get; set; } = new List<ReglesCriticite>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
