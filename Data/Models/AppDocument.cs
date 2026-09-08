using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class AppDocument
{
    public int Id { get; set; }

    public string SourceId { get; set; } = null!;

    public string Titre { get; set; } = null!;

    public string Contenu { get; set; } = null!;

    public string TypeSource { get; set; } = null!;

    public int? IdApplication { get; set; }

    public virtual Application? IdApplicationNavigation { get; set; }
}
