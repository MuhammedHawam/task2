using System;

namespace GAIA.Api.Contracts
{
  public record AssessmentResponse(
    Guid Id,
    string Title,
    string Description,
    DateTime CreatedAt,
    Guid CreatedBy,
    Guid FrameworkId,
    Guid AssessmentDepthId,
    Guid AssessmentScoringId
  );
}
