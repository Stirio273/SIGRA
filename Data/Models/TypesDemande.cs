using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class TypesDemande
{
    public int IdTypeDemande { get; set; }

    public string Libelle { get; set; } = null!;

    public virtual ICollection<ReglesCriticite> ReglesCriticites { get; set; } = new List<ReglesCriticite>();

    public virtual ICollection<Sla> Slas { get; set; } = new List<Sla>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
