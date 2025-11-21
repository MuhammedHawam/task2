using GAIA.Domain.Assessment.DomainEvents;

namespace GAIA.Core.Assessment.Interfaces;

public interface IAssessmentFirstStepEventWriter
{
  Task<Guid> CreateAsync(AssessmentFirstStepCreated @event, CancellationToken cancellationToken);
  Task AppendAsync(Guid assessmentId, AssessmentFirstStepUpdated @event, CancellationToken cancellationToken);
}
