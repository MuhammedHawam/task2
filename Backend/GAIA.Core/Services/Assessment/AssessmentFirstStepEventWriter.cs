using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.DomainEvents;
using Marten;

namespace GAIA.Core.Services.Assessment
{
  public class AssessmentFirstStepEventWriter : IAssessmentFirstStepEventWriter
  {
    private readonly IDocumentSession _session;

    public AssessmentFirstStepEventWriter(IDocumentSession session)
    {
      _session = session;
    }

    public async Task<Guid> CreateAsync(AssessmentFirstStepCreated @event, CancellationToken cancellationToken)
    {
      if (@event is null)
      {
        throw new ArgumentNullException(nameof(@event));
      }

      if (@event.Id == Guid.Empty)
      {
        throw new ArgumentException("AssessmentFirstStepCreated.Id must be a non-empty GUID.", nameof(@event));
      }

      _session.Events.StartStream<Domain.Assessment.Entities.AssessmentFirstStep>(@event.Id, @event);
      await _session.SaveChangesAsync(cancellationToken);
      return @event.Id;
    }

    public async Task AppendAsync(Guid assessmentId, AssessmentFirstStepUpdated @event, CancellationToken cancellationToken)
    {
      if (@event is null)
      {
        throw new ArgumentNullException(nameof(@event));
      }

      if (assessmentId == Guid.Empty)
      {
        throw new ArgumentException("assessmentId must be a non-empty GUID.", nameof(assessmentId));
      }

      _session.Events.Append(assessmentId, @event);
      await _session.SaveChangesAsync(cancellationToken);
    }
  }
}
