using GAIA.Api.Contracts.Documents;
using GAIA.Api.Mappers;
using GAIA.Core.Documents.Interfaces;
using GAIA.Core.Documents.Models;
using Microsoft.AspNetCore.Mvc;

namespace GAIA.Api.Controllers;

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
  public async Task<ActionResult<DocumentsResponse>> GetAll(CancellationToken cancellationToken)
  {
    var documents = await _documentService.GetAllAsync(cancellationToken);
    var response = new DocumentsResponse(
      documents.Documents
        .Select(document => document.ToResponse())
        .ToList());

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

    return Ok(document.ToResponse());
  }

  [HttpPost]
  public async Task<ActionResult<DocumentResponse>> Create([FromBody] CreateDocumentRequest request, CancellationToken cancellationToken)
  {
    if (!TryDecodeBase64(request.Content, out var content))
    {
      ModelState.AddModelError(nameof(request.Content), "Content must be a valid base64 string.");
      return ValidationProblem(ModelState);
    }

    var documentId = await _documentService.CreateAsync(
      new CreateDocumentDto(content, request.Status, request.Category, request.Name),
      cancellationToken);

    var document = await _documentService.GetByIdAsync(documentId, cancellationToken);

    if (document is null)
    {
      return CreatedAtAction(nameof(GetById), new { id = documentId }, new { id = documentId });
    }

    return CreatedAtAction(nameof(GetById), new { id = documentId }, document.ToResponse());
  }

  [HttpPost("upload")]
  public async Task<ActionResult<DocumentResponse>> Upload([FromForm] UploadDocumentRequest request, CancellationToken cancellationToken)
  {
    if (request.File is null || request.File.Length == 0)
    {
      ModelState.AddModelError(nameof(request.File), "File is required.");
      return ValidationProblem(ModelState);
    }

    await using var stream = new MemoryStream();
    await request.File.CopyToAsync(stream, cancellationToken);
    var content = stream.ToArray();

    var documentId = await _documentService.CreateAsync(
      new CreateDocumentDto(content, request.Status, request.Category, request.Name),
      cancellationToken);

    var document = await _documentService.GetByIdAsync(documentId, cancellationToken);

    if (document is null)
    {
      return CreatedAtAction(nameof(GetById), new { id = documentId }, new { id = documentId });
    }

    return CreatedAtAction(nameof(GetById), new { id = documentId }, document.ToResponse());
  }

  [HttpPut("{id:guid}")]
  public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentRequest request, CancellationToken cancellationToken)
  {
    byte[]? content = null;

    if (request.Content is not null)
    {
      if (!TryDecodeBase64(request.Content, out var decoded))
      {
        ModelState.AddModelError(nameof(request.Content), "Content must be a valid base64 string.");
        return ValidationProblem(ModelState);
      }

      content = decoded;
    }

    var updated = await _documentService.UpdateAsync(
      id,
      new UpdateDocumentDto(request.Status, request.Category, request.Name, content),
      cancellationToken);

    if (!updated)
    {
      return NotFound();
    }

    return NoContent();
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

  private static bool TryDecodeBase64(string value, out byte[] content)
  {
    try
    {
      content = Convert.FromBase64String(value);
      return true;
    }
    catch (FormatException)
    {
      content = Array.Empty<byte>();
      return false;
    }
  }
}
