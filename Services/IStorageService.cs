namespace SIGRA.Services;

public interface IStorageService
{
    Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folder);

    Task DeleteAsync(string fileUrl);
    Task<Stream> DownloadAsync(string fileUrl);
}