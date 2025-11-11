namespace GAIA.Domain.Assessment.Entities
{
  public class AssessmentScoring
  {
    public Guid Id { get; set; }
    public Guid FrameworkId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
  }
}
