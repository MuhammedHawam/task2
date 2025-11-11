namespace GAIA.Domain.Assessment.Entities
{
  public class AssessmentScoring
  {
    public Guid Id { get; set; }
    public required Guid FrameworkId { get; set; }
    public Guid AssessmentDepthId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
  }
}
