namespace GAIA.Domain.Assessment.Entities;

// TODO: Refactor all usages to Framework.Scoring
[Obsolete("Deprecated, use GAIA.Domain.Framework.Scoring instead.")]
public class AssessmentScoring
{
  public Guid Id { get; set; }
  public Guid FrameworkId { get; set; }
  public required string Name { get; set; }
  public string? Description { get; set; }
}
