using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MimeKit;
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

    public async Task<string> UploadFromEmailAsync(
            MimeContent mimeContent,
            string fileName,
            string contentType,
            string folder)
    {
        var folderPath = Path.Combine(_options.BasePath, folder);
        Directory.CreateDirectory(folderPath);

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        using var outputStream = File.Create(filePath);
        mimeContent.DecodeTo(outputStream);

        _logger.LogInformation("Fichier uploadé : {FilePath}", filePath);

        return $"{_options.BaseUrl}/{folder}/{uniqueFileName}";
    }

    public async Task<string> UploadAsync(IFormFile file, string folder)
    {
        var folderPath = Path.Combine(_options.BasePath, folder);
        Directory.CreateDirectory(folderPath);

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        await using var stream = File.Create(filePath);
        await file.CopyToAsync(stream);

        _logger.LogInformation("Fichier uploadé : {FilePath}", filePath);

        return $"{_options.BaseUrl}/{folder}/{uniqueFileName}";
    }

    public async Task DeleteAsync(string fileUrl)
    {
        var relativePath = fileUrl.Replace(_options.BaseUrl, "");
        var filePath = Path.Combine(_options.BasePath, relativePath);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation(
                "Fichier supprimé : {FilePath}", filePath);
        }
    }

    public Task<Stream> DownloadAsync(string relativePath)
    {
        var filePath = Path.Combine(_options.BasePath, relativePath);

        if (!File.Exists(filePath))
            throw new NotFoundException($"Fichier introuvable : {filePath}");

        Stream stream = File.OpenRead(filePath);
        return Task.FromResult(stream);
    }
}
