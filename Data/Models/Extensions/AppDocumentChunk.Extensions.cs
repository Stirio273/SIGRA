using Pgvector;

namespace SIGRA.Data.Models;

public partial class AppDocumentChunk
{
    public Vector? Embedding { get; set; }
}