using MediatR;

namespace GAIA.Core.Assessment.Commands.Documents;

public record AddEvidenceDocumentCommand(
  Guid AssessmentId,
  Guid UploadedBy,
  string FileName,
  string ContentType,
  long FileSize,
  byte[] Content,
  string Category = "Evidence",
  string? Description = null
) : IRequest<AddEvidenceDocumentResult?>;

public record AddEvidenceDocumentResult(
  Guid AssessmentId,
  Guid EvidenceDocumentId,
  Guid FileId,
  string FileName,
  string ContentType,
  long FileSize,
  string Category,
  string? Description,
  DateTime UploadedAt,
  Guid UploadedBy
);
