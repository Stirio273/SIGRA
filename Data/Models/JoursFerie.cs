using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class JoursFerie
{
    public int IdJourFerie { get; set; }

    public DateOnly Date { get; set; }

    public string Libelle { get; set; } = null!;
}
