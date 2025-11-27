namespace GAIA.Core.Assessment.Queries;

public record AssessmentEvidenceDocumentModel(
  Guid EvidenceDocumentId,
  Guid FileId,
  string FileName,
  string ContentType,
  long FileSize,
  string Category,
  string? Description,
  Guid UploadedBy,
  DateTime UploadedAt
);
