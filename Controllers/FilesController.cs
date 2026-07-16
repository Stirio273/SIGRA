using Microsoft.AspNetCore.Mvc;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("files")]
public class FilesController : ControllerBase
{
    private readonly IStorageService _storageService;

    public FilesController(IStorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpGet("{**filePath}")]
    public async Task<IActionResult> GetFileAsync(string filePath)
    {
        var stream = await _storageService.DownloadAsync(filePath);
        var contentType = GetContentType(filePath);

        return File(stream, contentType);
    }

    private string GetContentType(string filePath) =>
        Path.GetExtension(filePath).ToLower() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".docx" => "application/msword",
            _ => "application/octet-stream"
        };
}