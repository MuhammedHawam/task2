namespace GAIA.Domain.Assessment.Entities
{
  public class AssessmentDepth
  {
    public Guid Id { get; set; }
    public Guid FrameworkId { get; set; }
    public required string Name { get; set; }
    public int Depth { get; set; }
  }
}
