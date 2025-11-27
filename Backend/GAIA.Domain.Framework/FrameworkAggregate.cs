using GAIA.Domain;

namespace GAIA.Domain.Framework;

public class Framework : IdentityObject
{
  public required string Title { get; set; }
  public required string Description { get; set; }
  public required DateTime CreatedAt { get; set; }
  public required string CreatedBy { get; set; }
  public required FrameworkNode Root { get; set; }
  public required ICollection<FrameworkDepth> Depths { get; set; } = new List<FrameworkDepth>();
  public required ICollection<Scoring> Scorings { get; set; } = new List<Scoring>();
}

public class FrameworkNode : IdentityObject
{
  public string? Content { get; set; }
  public ICollection<FrameworkNode> Children { get; set; } = new List<FrameworkNode>();
}

public class FrameworkDepth : IdentityObject
{
  public required string Name { get; set; }
  public required int Depth { get; set; }
}

public class Scoring : IdentityObject
{
  public required string Name { get; set; }
  public string? Description { get; set; }
}
