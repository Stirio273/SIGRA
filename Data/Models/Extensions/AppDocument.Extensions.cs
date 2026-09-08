using Pgvector;

namespace SIGRA.Data.Models;

public partial class AppDocument
{
    public Vector? Embedding { get; set; }
}