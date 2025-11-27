namespace GAIA.Domain.InsightContent.DomainEvents;

public record UserUpdatedInsightEvent
{
  public Guid InsightId { get; init; }
  public string Content { get; init; } = string.Empty;
  public DateTime Timestamp { get; init; }
}
