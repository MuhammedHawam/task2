using GAIA.Domain.Assessment.Entities;

namespace GAIA.Core.Assessment.Queries;

public record AssessmentDetails(
  Domain.Assessment.Entities.Assessment Assessment,
  AssessmentDepth? Depth,
  AssessmentScoring? Scoring
);
