using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.DomainEvents;
using Marten;

namespace GAIA.Core.Services.Assessment
{
  public class AssessmentEventWriter : IAssessmentEventWriter
  {
    private readonly IDocumentSession _session;

    public AssessmentEventWriter(IDocumentSession session)
    {
      _session = session;
    }
    public async Task<Guid> CreateAsync(AssessmentCreated @event, CancellationToken cancellationToken)
    {
      if (@event is null)
      {
        throw new ArgumentNullException(nameof(@event));
      }

      if (@event.Id == Guid.Empty)
      {
        throw new ArgumentException("AssessmentCreated.Id must be a non-empty GUID.", nameof(@event));
      }

      _session.Events.StartStream<Domain.Assessment.Entities.Assessment>(@event.Id, @event);
      await _session.SaveChangesAsync(cancellationToken);
      return @event.Id;
    }
  }
}
