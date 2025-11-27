using GAIA.Api.Contracts.Document;
using GAIA.Domain.Document.Entities;
using GAIA.Infra.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GAIA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
  private readonly GaiaDbContext _context;

  public DocumentController(GaiaDbContext context)
  {
    _context = context;
  }

  /// <summary>
  /// Get all documents
  /// </summary>
  [HttpGet]
  public async Task<ActionResult<DocumentsStatusCategoryListResponse>> GetAll(CancellationToken cancellationToken)
  {
    var documents = await _context.Documents
        .AsNoTracking()
        .OrderBy(d => d.CreatedAt)
        .ToListAsync(cancellationToken);

    var response = new DocumentsStatusCategoryListResponse(
      documents.Select(d => new DocumentStatusCategoryResponse(
        d.Id,
        d.Status,
        d.Category,
        d.Name
      )).ToList()
    );

    return Ok(response);
  }

  /// <summary>
  /// Get document by ID
  /// </summary>
  [HttpGet("{id:guid}")]
  public async Task<ActionResult<DocumentResponse>> GetById(Guid id, CancellationToken cancellationToken)
  {
    var document = await _context.Documents
        .AsNoTracking()
        .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    if (document == null)
    {
      return NotFound();
    }

    return Ok(new DocumentResponse(
      document.Id,
      document.Name,
      document.Status,
      document.Category,
      document.Content,
      document.CreatedAt,
      document.UpdatedAt
    ));
  }

  /// <summary>
  /// Create a new document
  /// </summary>
  [HttpPost]
  public async Task<ActionResult<DocumentResponse>> Create([FromBody] CreateDocumentRequest request, CancellationToken cancellationToken)
  {
    var document = new Document
    {
      Id = Guid.NewGuid(),
      Name = request.Name,
      Status = request.Status,
      Category = request.Category,
      Content = request.Content,
      CreatedAt = DateTime.UtcNow
    };

    _context.Documents.Add(document);
    await _context.SaveChangesAsync(cancellationToken);

    return CreatedAtAction(nameof(GetById), new { id = document.Id }, new DocumentResponse(
      document.Id,
      document.Name,
      document.Status,
      document.Category,
      document.Content,
      document.CreatedAt,
      document.UpdatedAt
    ));
  }

  /// <summary>
  /// Update an existing document
  /// </summary>
  [HttpPut("{id:guid}")]
  public async Task<ActionResult<DocumentResponse>> Update(Guid id, [FromBody] UpdateDocumentRequest request, CancellationToken cancellationToken)
  {
    var document = await _context.Documents
        .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    if (document == null)
    {
      return NotFound();
    }

    if (request.Name != null)
    {
      document.Name = request.Name;
    }

    if (request.Status != null)
    {
      document.Status = request.Status;
    }

    if (request.Category != null)
    {
      document.Category = request.Category;
    }

    if (request.Content != null)
    {
      document.Content = request.Content;
    }

    document.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync(cancellationToken);

    return Ok(new DocumentResponse(
      document.Id,
      document.Name,
      document.Status,
      document.Category,
      document.Content,
      document.CreatedAt,
      document.UpdatedAt
    ));
  }

  /// <summary>
  /// Delete a document
  /// </summary>
  [HttpDelete("{id:guid}")]
  public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
  {
    var document = await _context.Documents
        .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    if (document == null)
    {
      return NotFound();
    }

    _context.Documents.Remove(document);
    await _context.SaveChangesAsync(cancellationToken);

    return NoContent();
  }

  /// <summary>
  /// Get status and category for all documents
  /// </summary>
  [HttpGet("status-category")]
  public async Task<ActionResult<DocumentsStatusCategoryListResponse>> GetStatusAndCategory(CancellationToken cancellationToken)
  {
    var documents = await _context.Documents
        .AsNoTracking()
        .OrderBy(d => d.CreatedAt)
        .ToListAsync(cancellationToken);

    var response = new DocumentsStatusCategoryListResponse(
      documents.Select(d => new DocumentStatusCategoryResponse(
        d.Id,
        d.Status,
        d.Category,
        d.Name
      )).ToList()
    );

    return Ok(response);
  }

  /// <summary>
  /// Upload a document file
  /// </summary>
  [HttpPost("upload")]
  [Consumes("multipart/form-data")]
  public async Task<ActionResult<DocumentResponse>> UploadDocument(
    [FromForm] IFormFile file,
    [FromForm] string? name = null,
    [FromForm] string? status = null,
    [FromForm] string? category = null,
    CancellationToken cancellationToken = default)
  {
    if (file == null || file.Length == 0)
    {
      return BadRequest("No file uploaded.");
    }

    // Read file content as text
    string content;
    using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
    {
      content = await reader.ReadToEndAsync(cancellationToken);
    }

    // Use file name if name not provided
    var documentName = name ?? file.FileName;
    var documentStatus = status ?? "Pending";
    var documentCategory = category ?? "Uncategorized";

    var document = new Document
    {
      Id = Guid.NewGuid(),
      Name = documentName,
      Status = documentStatus,
      Category = documentCategory,
      Content = content,
      CreatedAt = DateTime.UtcNow
    };

    _context.Documents.Add(document);
    await _context.SaveChangesAsync(cancellationToken);

    return CreatedAtAction(nameof(GetById), new { id = document.Id }, new DocumentResponse(
      document.Id,
      document.Name,
      document.Status,
      document.Category,
      document.Content,
      document.CreatedAt,
      document.UpdatedAt
    ));
  }
}
