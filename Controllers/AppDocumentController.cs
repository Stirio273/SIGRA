using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGRA.Controllers.Helper;
using SIGRA.Data;
using SIGRA.Data.Enums;
using SIGRA.Data.Models;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/application-documents")]
public class AppDocumentController : ControllerBase
{
    private readonly TextExtractionService _textExtractionService;
    private readonly AppDocumentService _appDocumentService;
    private readonly IStorageService _storageService;


    public AppDocumentController(TextExtractionService textExtractionService, AppDocumentService appDocumentService, IStorageService storageService)
    {
        _textExtractionService = textExtractionService;
        _appDocumentService = appDocumentService;
        _storageService = storageService;
    }

    [HttpPost]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> AddDocument(
    [FromForm] AddAppDocumentForm form,
    CancellationToken cancellationToken)
    {
        if (form.Files is null || form.Files.Count == 0)
            return BadRequest("At least one file is required.");

        var uploads = new List<AddAppDocumentFileResult>(form.Files.Count);

        foreach (var file in form.Files)
        {
            if (file is null || file.Length == 0)
            {
                uploads.Add(new AddAppDocumentFileResult(null, file?.FileName ?? string.Empty, false, "A file is required."));
                continue;
            }

            var extractionResult = await _textExtractionService.ExtractAsync(
                file, cancellationToken);

            if (!extractionResult.Success)
            {
                uploads.Add(new AddAppDocumentFileResult(
                    null, file.FileName, false, extractionResult.FailureReason));
                continue;
            }

            var fileUrl = await _storageService.UploadAsync(file, "application-documents");

            var document = new AppDocument
            {
                Titre = string.IsNullOrWhiteSpace(form.Title) || form.Files.Count > 1
                ? Path.GetFileNameWithoutExtension(file.FileName)
                : form.Title,
                Contenu = extractionResult.PlainText!,
                NomFichier = file.FileName,
                Chemin = fileUrl,
                IdApplication = form.IdApplication,
            };

            await _appDocumentService.AddApplicationDocument(document, cancellationToken);

            uploads.Add(new AddAppDocumentFileResult(document.Id, file.FileName, true, null));
        }

        return Ok(new
        {
            Uploaded = uploads.Count(u => u.Success),
            Failed = uploads.Count(u => !u.Success),
            Files = uploads
        });
    }

    [HttpGet("{idApplication:int}")]
    public async Task<IActionResult> GetAll(
        int idApplication,
        [FromQuery] PagedRequest pagination,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var documents = await _appDocumentService.GetAllApplicationDocuments(
            idApplication, pagination, search, cancellationToken);

        return Ok(documents);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _appDocumentService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("download/{idAppDocument:int}")]
    public async Task<IActionResult> GetFileAsync(int idAppDocument)
    {
        var appDocument = await _appDocumentService.GetByIdAsync(idAppDocument);
        if (appDocument == null)
            return NotFound();

        var relativePath = appDocument.Chemin.TrimStart('/');
        var stream = await _storageService.DownloadAsync(relativePath);
        var contentType = FileHelper.GetContentType(appDocument.NomFichier);

        return File(stream, contentType);
    }
}