using GAIA.Domain.Assessment.DomainEvents;

namespace GAIA.Domain.Assessment.Entities;

public class Assessment
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public DateTime StartDate { get; set; }
  public DateTime EndDate { get; set; }
  public string Organization { get; set; } = string.Empty;
  public string Language { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }

  public void Apply(AssessmentCreated @event)
  {
    Id = @event.Id;
    Name = @event.Name;
    StartDate = @event.StartDate;
    EndDate = @event.EndDate;
    Organization = @event.Organization;
    Language = @event.Language;
    CreatedAt = @event.CreatedAt;
    UpdatedAt = null;
  }

  public void Apply(AssessmentUpdated @event)
  {
    Name = @event.Name;
    StartDate = @event.StartDate;
    EndDate = @event.EndDate;
    Organization = @event.Organization;
    Language = @event.Language;
    UpdatedAt = @event.UpdatedAt;
  }
}
