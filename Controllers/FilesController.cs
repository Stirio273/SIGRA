using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SIGRA.Data.Repositories;
using SIGRA.Domain.Options;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IStorageService _storageService;
    private readonly StorageOptions _storageOptions;
    private readonly IPiecesJointeRepository _piecesJointeRepository;

    public FilesController(
        IStorageService storageService,
        IOptions<StorageOptions> storageOptions,
        IPiecesJointeRepository piecesJointeRepository)
    {
        _storageService = storageService;
        _storageOptions = storageOptions.Value;
        _piecesJointeRepository = piecesJointeRepository;
    }

    [HttpGet("{idPieceJointe:int}")]
    public async Task<IActionResult> GetFileAsync(int idPieceJointe)
    {
        var pieceJointe = await _piecesJointeRepository.GetByIdAsync(idPieceJointe);
        if (pieceJointe == null)
            return NotFound();

        var relativePath = pieceJointe.Chemin.Replace(_storageOptions.BaseUrl, "").TrimStart('/');
        var stream = await _storageService.DownloadAsync(relativePath);
        var contentType = GetContentType(pieceJointe.NomFichier);

        return File(stream, contentType);
    }

    private string GetContentType(string filePath) =>
        Path.GetExtension(filePath).ToLower() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
}