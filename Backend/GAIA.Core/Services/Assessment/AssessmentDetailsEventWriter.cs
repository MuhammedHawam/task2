using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.DomainEvents;
using Marten;

namespace GAIA.Core.Services.Assessment
{
  public class AssessmentDetailsEventWriter : IAssessmentDetailsEventWriter
  {
    private readonly IDocumentSession _session;

    public AssessmentDetailsEventWriter(IDocumentSession session)
    {
      _session = session;
    }

    public async Task<Guid> CreateAsync(AssessmentDetailsCreated @event, CancellationToken cancellationToken)
    {
      if (@event is null)
      {
        throw new ArgumentNullException(nameof(@event));
      }

      if (@event.Id == Guid.Empty)
      {
        throw new ArgumentException("AssessmentDetailsCreated.Id must be a non-empty GUID.", nameof(@event));
      }

      _session.Events.StartStream<Domain.Assessment.Entities.AssessmentDetails>(@event.Id, @event);
      await _session.SaveChangesAsync(cancellationToken);
      return @event.Id;
    }
  }
}
