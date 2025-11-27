using MediatR;

namespace GAIA.Core.Assessment.Queries;

public record GetAssessmentDocumentsQuery(Guid AssessmentId) : IRequest<AssessmentDocumentsResult?>;

public record AssessmentDocumentsResult(
  Guid AssessmentId,
  IReadOnlyList<AssessmentEvidenceDocumentModel> Documents
);
