using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class ClasseDeService
{
    public int IdCs { get; set; }

    public string Code { get; set; } = null!;

    public string? Libelle { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<RegleCriticite> RegleCriticites { get; set; } = new List<RegleCriticite>();

    public virtual ICollection<Sla> Slas { get; set; } = new List<Sla>();
}
