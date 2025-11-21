using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.Assessment.Entities;
using Marten.Events.Aggregation;

namespace GAIA.Infra.Projections
{
  public class AssessmentFirstStepProjection : SingleStreamProjection<AssessmentFirstStep, Guid>
  {
    public void Apply(AssessmentFirstStepCreated @event, AssessmentFirstStep state)
    {
      state.Id = @event.Id;
      state.Name = @event.Name;
      state.StartDate = @event.StartDate;
      state.EndDate = @event.EndDate;
      state.Organization = @event.Organization;
      state.Language = @event.Language;
      state.CreatedAt = @event.CreatedAt;
      state.UpdatedAt = null;
    }

    public void Apply(AssessmentFirstStepUpdated @event, AssessmentFirstStep state)
    {
      state.Name = @event.Name;
      state.StartDate = @event.StartDate;
      state.EndDate = @event.EndDate;
      state.Organization = @event.Organization;
      state.Language = @event.Language;
      state.UpdatedAt = @event.UpdatedAt;
    }
  }
}
