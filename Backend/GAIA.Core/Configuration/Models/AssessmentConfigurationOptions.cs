using System;
using System.Collections.Generic;

namespace GAIA.Core.Configuration.Models
{
  public record AssessmentConfigurationOptions(IReadOnlyList<FrameworkConfigurationOption> Frameworks);

  public record FrameworkConfigurationOption(
    Guid Id,
    string Name,
    IReadOnlyList<AssessmentDepthOption> AssessmentDepths
  );

  public record AssessmentDepthOption(
    Guid Id,
    string Name,
    IReadOnlyList<AssessmentScoringOption> AssessmentScorings
  );

  public record AssessmentScoringOption(
    Guid Id,
    string Name
  );
}
