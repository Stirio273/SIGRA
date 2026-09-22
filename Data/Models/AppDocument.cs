using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class AppDocument
{
    public int Id { get; set; }

    public string Titre { get; set; } = null!;

    public string Contenu { get; set; } = null!;

    public int? IdApplication { get; set; }

    public string NomFichier { get; set; } = null!;

    public string Chemin { get; set; } = null!;

    public virtual ICollection<AppDocumentChunk> AppDocumentChunks { get; set; } = new List<AppDocumentChunk>();

    public virtual Application? IdApplicationNavigation { get; set; }
}
