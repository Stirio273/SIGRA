namespace SIGRA.Services;

public sealed class MockLlmClient : ILlmClient
{
    public Task<string> GetCompletionAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var response = """
            {
              "ticketUnderstanding": "The technician is investigating a stock valuation issue after a Purchase Receipt.",
              "suggestedSteps": [
                "Check the Stock Ledger Entries for the affected item and warehouse.",
                "Verify whether the Purchase Receipt was backdated.",
                "Confirm negative stock settings for the warehouse."
              ],
              "possibleCauses": [
                "Backdated stock transaction affecting valuation.",
                "Incorrect valuation method configured for the item."
              ],
              "recommendedEscalation": null,
              "limitationOrUncertainty": "This is a mock response used for testing before an SLM is connected."
            }
            """;

        return Task.FromResult(response);
    }
}
