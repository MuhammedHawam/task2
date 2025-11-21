namespace GAIA.Api.Contracts.Assessment;

/// <summary>
/// Represents the outcome of updating (or creating) Insight content.
/// </summary>
/// <param name="AssessmentId">Target assessment identifier.</param>
/// <param name="InsightId">Target Insight identifier.</param>
/// <param name="Content">Persisted content.</param>
/// <param name="CreatedNew">Indicates whether the Insight was created as part of this request.</param>
public record UpdateInsightContentResponse(
  Guid AssessmentId,
  Guid InsightId,
  string Content,
  bool CreatedNew
);
