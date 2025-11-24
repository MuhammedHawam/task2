using GAIA.Domain.InsightContent.DomainEvents;

namespace GAIA.Domain.InsightContent.Entities
{
  public class InsightContent
  {
    public Guid Id { get; set; } // Primary Key
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid AssessmentId { get; set; } // Foreign Key to AssessmentDetails

    // Apply method for domain events
    public void Apply(InsightContentCreated e)
    {
      Id = e.Id;
      Content = e.Content;
      CreatedAt = e.CreatedAt;
      CreatedBy = e.CreatedBy;
      AssessmentId = e.AssessmentId;
    }
  }
}
