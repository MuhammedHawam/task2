using GAIA.Core.Interfaces.Assessment;
using GAIA.Domain.DomainEvents;
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
      var streamId = @event.Id != Guid.Empty ? @event.Id : Guid.NewGuid();
      _session.Events.StartStream<Domain.Assessment.Entities.Assessment>(streamId, @event);
      await _session.SaveChangesAsync(cancellationToken);
      return streamId;
    }
  }
}
