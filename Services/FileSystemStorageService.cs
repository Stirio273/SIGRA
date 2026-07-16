using Microsoft.Extensions.Options;
using SIGRA.Domain.Exceptions;
using SIGRA.Domain.Options;

namespace SIGRA.Services;

public class FileSystemStorageService : IStorageService
{
    private readonly StorageOptions _options;
    private readonly ILogger<FileSystemStorageService> _logger;

    public FileSystemStorageService(
        IOptions<StorageOptions> options,
        ILogger<FileSystemStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder)
    {
        var folderPath = Path.Combine(_options.BasePath, folder);
        Directory.CreateDirectory(folderPath);

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        // Écriture du Stream peu importe sa source
        using var outputStream = File.Create(filePath);
        await fileStream.CopyToAsync(outputStream);

        _logger.LogInformation("Fichier uploadé : {FilePath}", filePath);

        return $"{_options.BaseUrl}/{folder}/{uniqueFileName}";
    }

    public async Task DeleteAsync(string fileUrl)
    {
        // Reconvertir l'URL en chemin local
        var relativePath = fileUrl.Replace(_options.BaseUrl, "");
        var filePath = Path.Combine(_options.BasePath, relativePath);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation(
                "Fichier supprimé : {FilePath}", filePath);
        }
    }

    public Task<Stream> DownloadAsync(string fileUrl)
    {
        // Reconvertir l'URL en chemin local
        var relativePath = fileUrl.Replace(_options.BaseUrl, "");
        var filePath = Path.Combine(_options.BasePath, relativePath);

        if (!File.Exists(filePath))
            throw new NotFoundException($"Fichier introuvable : {filePath}");

        // Fonctionne pour Local ET NFS
        Stream stream = File.OpenRead(filePath);
        return Task.FromResult(stream);
    }
}
