using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class Application
{
    public int IdApplication { get; set; }

    public string Libelle { get; set; } = null!;

    public bool Actif { get; set; }

    public int IdCs { get; set; }

    public virtual ClassesService IdCsNavigation { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
