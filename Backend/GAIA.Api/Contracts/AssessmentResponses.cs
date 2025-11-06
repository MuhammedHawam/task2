namespace GAIA.Api.Contracts;

public record AssessmentResponse(
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
  string? Description
);

public record AssessmentScoringResponse(
  Guid Id,
  Guid AssessmentDepthId,
  string Name,
  string? Description
);
