using GAIA.Api.Contracts.Assessment;
using GAIA.Core.Assessment.Queries;
using GAIA.Domain.Assessment.Entities;

namespace GAIA.Api.Mappers;

public static class AssessmentMapper
{
  public static AssessmentResponse ToResponse(this AssessmentDetails details) => new(
    details.Assessment.Id,
    details.Assessment.Title,
    details.Assessment.Description,
    details.Assessment.CreatedAt,
    details.Assessment.CreatedBy,
    details.Assessment.FrameworkId,
    details.Assessment.AssessmentDepthId,
    details.Assessment.AssessmentScoringId,
    details.Depth?.ToResponse(),
    details.Scoring?.ToResponse()
  );

  private static AssessmentDepthResponse ToResponse(this AssessmentDepth depth) => new(
    depth.Id,
    depth.FrameworkId,
    depth.Name,
    depth.Depth
  );

  private static AssessmentScoringResponse ToResponse(this AssessmentScoring scoring) => new(
    scoring.Id,
    scoring.FrameworkId,
    scoring.Name,
    scoring.Description
  );
}
