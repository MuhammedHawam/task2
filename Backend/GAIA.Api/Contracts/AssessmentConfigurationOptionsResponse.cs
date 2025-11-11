namespace GAIA.Api.Contracts
{
  public record AssessmentConfigurationOptionsResponse(IReadOnlyList<FrameworkOptionsDto> Frameworks);

  public record FrameworkOptionsDto(
    Guid Id,
    string Name,
    IReadOnlyList<AssessmentDepthOptionsDto> AssessmentDepths
  );

  public record AssessmentDepthOptionsDto(
    Guid Id,
    string Name,
    int Depth,
    IReadOnlyList<AssessmentScoringOptionsDto> AssessmentScorings
  );

  public record AssessmentScoringOptionsDto(
    Guid Id,
    string Name
  );
}
