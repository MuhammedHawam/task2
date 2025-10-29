namespace GAIA.Domain.Framework.DomainEvents
{
  public class FrameworkCreated
  {
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
  }

}
