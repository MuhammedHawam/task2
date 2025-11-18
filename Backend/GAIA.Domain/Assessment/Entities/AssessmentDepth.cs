namespace GAIA.Domain.Assessment.Entities;

// TODO: Refactor all usages to FrameworkDepth
[Obsolete("Deprecated, use GAIA.Domain.Framework.FrameworkDepth instead.")]
public class AssessmentDepth
{
  public Guid Id { get; set; }
  public Guid FrameworkId { get; set; }
  public required string Name { get; set; }
  public required int Depth { get; set; }
}
