using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public interface IKnowledgeDocumentStore
{
    IReadOnlyList<KnowledgeDocument> GetAll();
}

public sealed class InMemoryKnowledgeDocumentStore : IKnowledgeDocumentStore
{
    private readonly List<KnowledgeDocument> _documents;

    public InMemoryKnowledgeDocumentStore()
    {
        // Temporary: replace with real document loading later
        // (e.g., reading extracted text from your 6 PDFs/Word files).
        _documents =
        [
            new KnowledgeDocument
            {
                SourceId = "DOC-STOCK-001",
                Title = "Stock Valuation Troubleshooting",
                Module = "Stock",
                Content = """
                    If a stock valuation appears incorrect, check the Stock Ledger Entries
                    for the affected item and warehouse. Verify whether any backdated
                    Purchase Receipt or Stock Entry affected the valuation. Confirm the
                    valuation method (FIFO, Moving Average) configured for the item.
                    """
            },
            new KnowledgeDocument
            {
                SourceId = "DOC-BUYING-002",
                Title = "Purchase Receipt Processing Guide",
                Module = "Buying",
                Content = """
                    When a Purchase Receipt is submitted, ERPNext creates Stock Ledger
                    Entries reflecting the received quantity and rate. If the receipt
                    is backdated relative to existing transactions, reposting stock
                    may be required to correct valuation.
                    """
            }
            // Add remaining documents here.
        ];
    }

    public IReadOnlyList<KnowledgeDocument> GetAll() => _documents;
}
