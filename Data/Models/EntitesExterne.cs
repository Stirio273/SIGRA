using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class EntitesExterne
{
    public int IdEntiteExterne { get; set; }

    public string Nom { get; set; } = null!;

    public bool Actif { get; set; }

    public virtual ICollection<Escalade> Escalades { get; set; } = new List<Escalade>();
}
