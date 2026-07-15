using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class PiecesJointe
{
    public int IdPieceJointe { get; set; }

    public int IdEmailSource { get; set; }

    public string NomFichier { get; set; } = null!;

    public string Chemin { get; set; } = null!;

    public long? TailleOctets { get; set; }

    public string? TypeMime { get; set; }

    public virtual EmailsSource IdEmailSourceNavigation { get; set; } = null!;
}
