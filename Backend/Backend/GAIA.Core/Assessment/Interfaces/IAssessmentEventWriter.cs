using GAIA.Domain.DomainEvents;

namespace GAIA.Core.Assessment.Interfaces;

public interface IAssessmentEventWriter
{
  Task<Guid> CreateAsync(AssessmentCreated @event, CancellationToken cancellationToken);
}