using GAIA.Domain.Assessment.DomainEvents;

namespace GAIA.Core.Assessment.Interfaces;

public interface IAssessmentEventWriter
{
  Task<Guid> CreateAsync(AssessmentCreated @event, CancellationToken cancellationToken);
}
