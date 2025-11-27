using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.Assessment.Entities;
using Marten.Events.Aggregation;

namespace GAIA.Infra.Projections;

public class AssessmentUserAssignmentProjection : SingleStreamProjection<AssessmentUserAssignment, Guid>
{
  public void Apply(AssessmentUsersAssigned e, AssessmentUserAssignment state)
  {
    state.Apply(e);
  }

  public void Apply(AssessmentUsersUpdated e, AssessmentUserAssignment state)
  {
    state.Apply(e);
  }

  public void Apply(AssessmentUsersRemoved e, AssessmentUserAssignment state)
  {
    state.Apply(e);
  }
}
