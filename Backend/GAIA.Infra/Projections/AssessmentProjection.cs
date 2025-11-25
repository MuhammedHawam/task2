using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.Assessment.Entities;
using Marten.Events.Aggregation;

namespace GAIA.Infra.Projections;

public class AssessmentProjection : SingleStreamProjection<Assessment, Guid>
{
  public void Apply(AssessmentCreated @event, Assessment state)
  {
    state.Id = @event.Id;
    state.Name = @event.Name;
    state.StartDate = @event.StartDate;
    state.EndDate = @event.EndDate;
    state.OrganizationId = @event.OrganizationId;
    state.Organization = @event.Organization;
    state.Language = @event.Language;
    state.CreatedAt = @event.CreatedAt;
    state.UpdatedAt = null;
  }

  public void Apply(AssessmentUpdated @event, Assessment state)
  {
    state.Name = @event.Name;
    state.StartDate = @event.StartDate;
    state.EndDate = @event.EndDate;
    state.OrganizationId = @event.OrganizationId;
    state.Organization = @event.Organization;
    state.Language = @event.Language;
    state.UpdatedAt = @event.UpdatedAt;
  }
}
