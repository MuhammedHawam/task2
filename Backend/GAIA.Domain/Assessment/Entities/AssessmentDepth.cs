namespace GAIA.Domain.Assessment.Entities
{
  public class AssessmentDepth
  {
    public Guid Id { get; set; }
    public Guid FrameworkId { get; set; }
    public required string Name { get; set; } = string.Empty;
    public required int Depth { get; set; }
  }
}
