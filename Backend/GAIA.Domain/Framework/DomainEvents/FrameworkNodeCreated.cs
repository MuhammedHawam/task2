namespace GAIA.Domain.Framework.DomainEvents
{
  public class FrameworkNodeCreated
  {
    public Guid Id { get; set; }
    public int Depth { get; set; }
    public string Content { get; set; }
  }
}
