namespace GAIA.Domain.InsightContent.DomainEvents;

public record UserUpdatedInsightEvent
{
  public Guid AssessmentId { get; init; }
  public Guid InsightId { get; init; }
  public string Content { get; init; } = string.Empty;
  public DateTime OccurredAt { get; init; }
}
