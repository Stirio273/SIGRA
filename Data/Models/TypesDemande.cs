using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class TypesDemande
{
    public int IdTypeDemande { get; set; }

    public string Libelle { get; set; } = null!;
}
