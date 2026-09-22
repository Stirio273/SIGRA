using System;
using System.Collections.Generic;
using Pgvector;

namespace SIGRA.Data.Models;

public partial class AppDocumentChunk
{
    public int Id { get; set; }

    public int ParentId { get; set; }

    public int ChunkIndex { get; set; }

    public string Content { get; set; } = null!;

    public Vector Embedding { get; set; } = null!;

    public virtual AppDocument Parent { get; set; } = null!;
}
