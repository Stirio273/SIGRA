using MimeKit;

namespace SIGRA.Services;

public interface IStorageService
{
    Task<string> UploadFromEmailAsync(
        MimeContent mimeContent,
        string fileName,
        string contentType,
        string folder);

    Task DeleteAsync(string fileUrl);
    Task<Stream> DownloadAsync(string fileUrl);
}