namespace GAIA.Api.Contracts.Assessment
{
    public record AssessmentConfigurationOptionsResponse(IReadOnlyList<FrameworkOptionsDto> Frameworks);

    public record FrameworkOptionsDto(
      Guid Id,
      string Name,
      IReadOnlyList<AssessmentDepthOptionsDto> AssessmentDepths,
      IReadOnlyList<AssessmentScoringOptionsDto> AssessmentScorings
    );

    public record AssessmentDepthOptionsDto(
      Guid Id,
      string Name,
      int Depth
    );

    public record AssessmentScoringOptionsDto(
      Guid Id,
      string Name
    );
}