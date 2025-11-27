namespace GAIA.Api.Contracts.Assessment.EvidenceDocuments;

public record AssessmentEvidenceDocumentResponse(
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

public record AssessmentEvidenceDocumentListResponse(
  IReadOnlyList<AssessmentEvidenceDocumentResponse> Documents
);
