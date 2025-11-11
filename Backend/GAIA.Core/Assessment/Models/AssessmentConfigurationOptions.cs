namespace GAIA.Core.Assessment.Models
{
  public record AssessmentConfigurationOptions(IReadOnlyList<FrameworkConfigurationOption> Frameworks);

  public record FrameworkConfigurationOption(
    Guid Id,
    string Name,
    IReadOnlyList<AssessmentDepthOption> AssessmentDepths,
    IReadOnlyList<AssessmentScoringOption> AssessmentScorings
  );

  public record AssessmentDepthOption(
    Guid Id,
    string Name,
    int Depth
  );

  public record AssessmentScoringOption(
    Guid Id,
    string Name
  );
}
