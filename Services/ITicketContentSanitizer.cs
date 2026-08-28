namespace SIGRA.Services;

public interface ITicketContentSanitizer
{
    string Sanitize(string? content);
}
