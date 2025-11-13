using GAIA.Api.Contracts.Documents;
using GAIA.Core.Documents.Interfaces;
using GAIA.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace GAIA.Api.Controllers
{
  [ApiController]
  [Route("documents")]
  public class DocumentsController : ControllerBase
  {
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
      _documentService = documentService;
    }

    [HttpGet]
    public async Task<ActionResult<DocumentListResponse>> GetDocuments(CancellationToken cancellationToken)
    {
      var documents = await _documentService.GetSummariesAsync(cancellationToken);
      var response = new DocumentListResponse(
        documents.Select(document => new DocumentSummaryResponse(document.Id, document.Status, document.Category, document.Name)).ToList());

      return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
      var document = await _documentService.GetByIdAsync(id, cancellationToken);

      if (document is null)
      {
        return NotFound();
      }

      return Ok(ToResponse(document));
    }

    [HttpPost]
    public async Task<ActionResult<DocumentResponse>> Create([FromBody] CreateDocumentRequest request, CancellationToken cancellationToken)
    {
      if (!TryDecodeBase64(request.ContentBase64, out var content))
      {
        return BadRequest("ContentBase64 must be a valid Base64 encoded string.");
      }

      var document = await _documentService.CreateAsync(request.Name, request.Category, request.Status, content, cancellationToken);
      var response = ToResponse(document);
      return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPost("upload")]
    [RequestFormLimits(MultipartBodyLengthLimit = 52428800)]
    public async Task<ActionResult<DocumentResponse>> Upload([FromForm] UploadDocumentRequest request, CancellationToken cancellationToken)
    {
      if (request.File is null || request.File.Length == 0)
      {
        return BadRequest("File must not be empty.");
      }

      await using var memoryStream = new MemoryStream();
      await request.File.CopyToAsync(memoryStream, cancellationToken);
      var document = await _documentService.CreateAsync(request.Name, request.Category, request.Status, memoryStream.ToArray(), cancellationToken);
      var response = ToResponse(document);
      return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DocumentResponse>> Update(Guid id, [FromBody] UpdateDocumentRequest request, CancellationToken cancellationToken)
    {
      byte[]? content = null;
      if (!string.IsNullOrWhiteSpace(request.ContentBase64))
      {
        if (!TryDecodeBase64(request.ContentBase64, out var decoded))
        {
          return BadRequest("ContentBase64 must be a valid Base64 encoded string.");
        }

        content = decoded;
      }

      var document = await _documentService.UpdateAsync(id, request.Name, request.Category, request.Status, content, cancellationToken);
      if (document is null)
      {
        return NotFound();
      }

      return Ok(ToResponse(document));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
      var deleted = await _documentService.DeleteAsync(id, cancellationToken);
      if (!deleted)
      {
        return NotFound();
      }

      return NoContent();
    }

    private static bool TryDecodeBase64(string base64, out byte[] content)
    {
      try
      {
        content = Convert.FromBase64String(base64);
        return true;
      }
      catch (FormatException)
      {
        content = Array.Empty<byte>();
        return false;
      }
    }

    private static DocumentResponse ToResponse(Document document)
    {
      return new DocumentResponse(
        document.Id,
        document.Status,
        document.Category,
        document.Name,
        Convert.ToBase64String(document.Content));
    }
  }
}
