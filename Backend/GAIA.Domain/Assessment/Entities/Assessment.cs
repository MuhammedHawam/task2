using GAIA.Domain.Assessment.DomainEvents;

namespace GAIA.Domain.Assessment.Entities
{
  public class Assessment
  {
    public Guid Id { get; set; } // Primary Key
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid FrameworkId { get; set; } // Foreign Key to Framework

    // Apply method for domain events
    public void Apply(AssessmentCreated e)
    {
      Id = e.Id;
      Title = e.Title;
      Description = e.Description;
      CreatedAt = e.CreatedAt;
      CreatedBy = e.CreatedBy;
      FrameworkId = e.FrameworkId;
    }
  }
}
