using GAIA.Domain.Assessment.Entities;

namespace GAIA.Core.Assessment.Queries
{
  // TODO: Refactor circular dependency Domain -> Core -> Domain.
  public record AssessmentDetails(
    Domain.Assessment.Entities.AssessmentDetails Assessment,
    AssessmentDepth? Depth,
    AssessmentScoring? Scoring
  );
}
