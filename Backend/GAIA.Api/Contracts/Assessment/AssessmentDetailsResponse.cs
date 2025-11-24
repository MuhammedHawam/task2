namespace GAIA.Api.Contracts.Assessment;

public record AssessmentDetailsResponse(
  Guid Id,
  string Title,
  string Description,
  DateTime CreatedAt,
  Guid CreatedBy,
  Guid FrameworkId,
  Guid AssessmentDepthId,
  Guid AssessmentScoringId,
  AssessmentDepthResponse? Depth,
  AssessmentScoringResponse? Scoring
);

public record AssessmentDepthResponse(
  Guid Id,
  Guid FrameworkId,
  string Name,
  int Depth
);

public record AssessmentScoringResponse(
  Guid Id,
  Guid FrameworkId,
  string Name,
  string? Description
);
