using GAIA.Domain.Assessment.Entities;

namespace GAIA.Core.Assessment.Queries;

public record AssessmentDetails(
  Assessment Assessment,
  AssessmentDepth? Depth,
  AssessmentScoring? Scoring
);
