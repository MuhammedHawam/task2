namespace GAIA.Core.DTOs;

public record FrameworkConfigurationOption(
  Guid Id,
  string Name,
  IEnumerable<FrameworkDepthOption> AssessmentDepths,
  IEnumerable<ScoringOption> AssessmentScorings
);

public record FrameworkDepthOption(
  Guid Id,
  string Name,
  int Depth
);

public record ScoringOption(
  Guid Id,
  string Name
);
