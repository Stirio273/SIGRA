using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class ReglesCriticite
{
    public int IdRegleCriticite { get; set; }

    public int IdCs { get; set; }

    public int IdCriticite { get; set; }

    public virtual Criticite IdCriticiteNavigation { get; set; } = null!;

    public virtual ClassesService IdCsNavigation { get; set; } = null!;
}
