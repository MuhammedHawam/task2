namespace GAIA.Domain.Framework.DomainEvents
{
  public class FrameworkCreated
  {
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
  }

}
