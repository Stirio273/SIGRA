namespace SIGRA.Domain.Options;

public class SMTPOptions
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public bool EnableSsl { get; set; } = true;
    public string FromAddress { get; set; } = default!;
    public string[] Recipients { get; set; } = Array.Empty<string>();
}
