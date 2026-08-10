using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class ClassesService
{
    public int IdCs { get; set; }

    public string Code { get; set; } = null!;

    public string? Libelle { get; set; }

    public decimal DureeSla { get; set; }

    public int IdCriticite { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual Criticite IdCriticiteNavigation { get; set; } = null!;

    public virtual ICollection<ReglesCriticite> ReglesCriticites { get; set; } = new List<ReglesCriticite>();
}
