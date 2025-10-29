using GAIA.Domain.Framework.DomainEvents;

namespace GAIA.Domain.Framework.Entities
{
  public class Framework
  {
    public Guid Id { get; set; } // Primary Key
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public FrameworkNode Root { get; set; } // Root FrameworkNode    
    
    // Apply method for domain events
    public void Apply(FrameworkCreated e)
    {
      Id = e.Id;
      Title = e.Title;
      Description = e.Description;
      CreatedAt = e.CreatedAt;
      CreatedBy = e.CreatedBy;
    }
  }
}
