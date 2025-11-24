namespace GAIA.Domain.Assessment.DomainEvents;

public class AssessmentUpdated
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public DateTime StartDate { get; set; }
  public DateTime EndDate { get; set; }
  public string Organization { get; set; } = string.Empty;
  public string Language { get; set; } = string.Empty;
  public DateTime UpdatedAt { get; set; }
}
