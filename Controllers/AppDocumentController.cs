using Microsoft.AspNetCore.Mvc;
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

    public AppDocumentController(TextExtractionService textExtractionService, AppDocumentService appDocumentService)
    {
        _textExtractionService = textExtractionService;
        _appDocumentService = appDocumentService;
    }

    [HttpPost]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> AddDocument(
    [FromForm] AddAppDocumentForm form,
    CancellationToken cancellationToken)
    {
        if (form.File is null || form.File.Length == 0)
            return BadRequest("A file is required.");

        var extractionResult = await _textExtractionService.ExtractAsync(
            form.File, cancellationToken);

        if (!extractionResult.Success)
        {
            return UnprocessableEntity(new
            {
                error = "Could not extract text from this file.",
                reason = extractionResult.FailureReason
            });
        }

        var document = new AppDocument
        {
            Titre = string.IsNullOrWhiteSpace(form.Title)
            ? Path.GetFileNameWithoutExtension(form.File.FileName)
            : form.Title,
            Contenu = extractionResult.PlainText!,
            // Module = form.Module,
            // TypeSource = KnowledgeSourceType.Documentation.ToString(),
            IdApplication = form.IdApplication,
            // OriginalFileName = form.File.FileName
        };

        await _appDocumentService.AddApplicationDocument(document);

        return Ok(new { document.Id });
    }

}