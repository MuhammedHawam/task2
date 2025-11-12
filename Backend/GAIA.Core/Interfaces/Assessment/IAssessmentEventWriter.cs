using GAIA.Domain.Assessment.DomainEvents;

namespace GAIA.Core.Interfaces.Assessment
{
  public interface IAssessmentEventWriter
  {
    Task<Guid> CreateAsync(AssessmentCreated @event, CancellationToken cancellationToken);
  }
}
