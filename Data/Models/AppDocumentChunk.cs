using System;
using System.Collections.Generic;

namespace SIGRA.Data.Models;

public partial class AppDocumentChunk
{
    public int Id { get; set; }

    public string ParentSourceId { get; set; } = null!;

    public int ChunkIndex { get; set; }

    public string Content { get; set; } = null!;
}
