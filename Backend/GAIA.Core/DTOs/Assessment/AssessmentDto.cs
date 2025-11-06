using System;

namespace GAIA.Core.DTOs.Assessment
{
  public record AssessmentDto(
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
