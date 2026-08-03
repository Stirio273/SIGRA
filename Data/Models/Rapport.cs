using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class Rapport
{
    public int Id { get; set; }

    public DateTime DateDebutSemaine { get; set; }

    public string TypeRapport { get; set; } = null!;

    public DateTime DateEnvoie { get; set; }
}
