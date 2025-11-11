namespace GAIA.Domain.Assessment.Entities
{
  public class AssessmentDepth
  {
    public Guid Id { get; set; }
    public Guid FrameworkId { get; set; }
    public required int Depth { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
  }
}
