using GAIA.Api.Contracts.Assessment.EvidenceDocument;
using GAIA.Core.Assessment.Commands.Documents;
using GAIA.Core.Assessment.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GAIA.Api.Controllers;

[ApiController]
[Route("assessments/{assessmentId:guid}/documents")]
public class AssessmentDocumentsController : ControllerBase
{
  private readonly ISender _sender;

  public AssessmentDocumentsController(ISender sender)
  {
    _sender = sender;
  }

  [HttpPost]
  [Consumes("multipart/form-data")]
  public async Task<ActionResult<AssessmentEvidenceDocumentResponse>> UploadDocument(
    Guid assessmentId,
    [FromForm] UploadAssessmentEvidenceDocumentRequest request,
    CancellationToken cancellationToken)
  {
    if (request.File is null || request.File.Length == 0)
    {
      return BadRequest("Evidence document file must be provided.");
    }

    await using var memoryStream = new MemoryStream();
    await request.File.CopyToAsync(memoryStream, cancellationToken);

    var command = new AddEvidenceDocumentCommand(
      assessmentId,
      request.UploadedBy,
      request.File.FileName,
      request.File.ContentType ?? "application/octet-stream",
      request.File.Length,
      memoryStream.ToArray(),
      request.Category,
      request.Description
    );

    var result = await _sender.Send(command, cancellationToken);
    if (result is null)
    {
      return NotFound();
    }

    var response = ToResponse(result);
    return CreatedAtAction(nameof(GetDocuments), new { assessmentId }, response);
  }

  [HttpGet]
  public async Task<ActionResult<AssessmentEvidenceDocumentListResponse>> GetDocuments(
    Guid assessmentId,
    CancellationToken cancellationToken)
  {
    var result = await _sender.Send(new GetAssessmentDocumentsQuery(assessmentId), cancellationToken);
    if (result is null)
    {
      return NotFound();
    }

    var response = new AssessmentEvidenceDocumentListResponse(
      result.Documents.Select(ToResponse).ToList()
    );

    return Ok(response);
  }

  private static AssessmentEvidenceDocumentResponse ToResponse(AddEvidenceDocumentResult result) =>
    new(result.EvidenceDocumentId, result.FileId, result.FileName, result.ContentType, result.FileSize,
      result.Category, result.Description, result.UploadedBy, result.UploadedAt);

  private static AssessmentEvidenceDocumentResponse ToResponse(AssessmentEvidenceDocumentModel model) =>
    new(model.EvidenceDocumentId, model.FileId, model.FileName, model.ContentType, model.FileSize, model.Category,
      model.Description, model.UploadedBy, model.UploadedAt);
}
