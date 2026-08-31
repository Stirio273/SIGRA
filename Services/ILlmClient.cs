namespace SIGRA.Services;

public interface ILlmClient
{
    Task<string> GetCompletionAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);
}
