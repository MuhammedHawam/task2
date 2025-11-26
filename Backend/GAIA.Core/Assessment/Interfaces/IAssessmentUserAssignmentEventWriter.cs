using GAIA.Domain.Assessment.DomainEvents;

namespace GAIA.Core.Assessment.Interfaces;

public interface IAssessmentUserAssignmentEventWriter
{
  Task<Guid> AssignAsync(AssessmentUsersAssigned @event, CancellationToken cancellationToken);
  Task<Guid> UpdateAsync(AssessmentUsersUpdated @event, CancellationToken cancellationToken);
  Task<Guid> RemoveAsync(AssessmentUsersRemoved @event, CancellationToken cancellationToken);
}
