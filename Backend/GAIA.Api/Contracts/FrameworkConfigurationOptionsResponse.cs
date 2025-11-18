namespace GAIA.Api.Contracts;

public record FrameworkConfigurationOptionsResponse(IReadOnlyList<FrameworkOptionsDto> Frameworks);

public record FrameworkOptionsDto(
  Guid Id,
  string Name,
  IReadOnlyList<FrameworkDepthOptionsDto> AssessmentDepths,
  IReadOnlyList<FrameworkScoringOptionsDto> AssessmentScorings
);

public record FrameworkDepthOptionsDto(
  Guid Id,
  string Name,
  int Depth
);

public record FrameworkScoringOptionsDto(
  Guid Id,
  string Name
);
