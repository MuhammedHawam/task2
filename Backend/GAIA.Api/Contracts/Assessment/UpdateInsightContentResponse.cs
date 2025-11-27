namespace GAIA.Api.Contracts.Assessment;

/// <summary>
/// Represents the outcome of updating Insight content.
/// </summary>
/// <param name="AssessmentId">Target assessment identifier.</param>
/// <param name="InsightId">Target Insight identifier.</param>
/// <param name="Content">Persisted content.</param>
public record UpdateInsightContentResponse(
  Guid AssessmentId,
  Guid InsightId,
  string Content
);
