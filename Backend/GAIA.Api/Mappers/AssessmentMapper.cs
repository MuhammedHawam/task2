using GAIA.Api.Contracts;
using GAIA.Domain.Assessment.Entities;

namespace GAIA.Api.Mappers;

public static class AssessmentMapper
{
  public static AssessmentResponse ToResponse(this Assessment assessment) => new(
    assessment.Id,
    assessment.Title,
    assessment.Description,
    assessment.CreatedAt,
    assessment.CreatedBy,
    assessment.FrameworkId,
    assessment.AssessmentDepthId,
    assessment.AssessmentScoringId
  );
}
