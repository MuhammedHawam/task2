using GAIA.Domain.Assessment.DomainEvents;

namespace GAIA.Core.Assessment.Interfaces;

public interface IAssessmentDetailsEventWriter
{
  Task<Guid> CreateAsync(AssessmentDetailsCreated @event, CancellationToken cancellationToken);
}
