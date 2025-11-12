using GAIA.Domain.Framework.DomainEvents;

namespace GAIA.Domain.Framework.Entities
{
  public class FrameworkNode
  {
    public Guid Id { get; set; } // Primary Key
    public int Depth { get; set; }
    public string Content { get; set; }
    public ICollection<FrameworkNode> Children { get; set; } = new List<FrameworkNode>();

    // Apply method for domain events
    public void Apply(FrameworkNodeCreated e)
    {
      Id = e.Id;
      Depth = e.Depth;
      Content = e.Content;
    }
  }
}
