using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class TypeDemande
{
    public int IdTypeDemande { get; set; }

    public string Libelle { get; set; } = null!;

    public virtual ICollection<RegleCriticite> RegleCriticites { get; set; } = new List<RegleCriticite>();

    public virtual ICollection<Sla> Slas { get; set; } = new List<Sla>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
