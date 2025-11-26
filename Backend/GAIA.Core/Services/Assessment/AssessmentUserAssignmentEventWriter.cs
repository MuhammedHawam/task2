using System;
using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.Assessment.Entities;
using Marten;

namespace GAIA.Core.Services.Assessment;

public class AssessmentUserAssignmentEventWriter : IAssessmentUserAssignmentEventWriter
{
  private readonly IDocumentSession _session;

  public AssessmentUserAssignmentEventWriter(IDocumentSession session)
  {
    _session = session;
  }

  public Task<Guid> AssignAsync(AssessmentUsersAssigned @event, CancellationToken cancellationToken)
  {
    return AppendAsync(@event.AssessmentId, @event, cancellationToken);
  }

  public Task<Guid> UpdateAsync(AssessmentUsersUpdated @event, CancellationToken cancellationToken)
  {
    return AppendAsync(@event.AssessmentId, @event, cancellationToken);
  }

  public Task<Guid> RemoveAsync(AssessmentUsersRemoved @event, CancellationToken cancellationToken)
  {
    return AppendAsync(@event.AssessmentId, @event, cancellationToken);
  }

  private async Task<Guid> AppendAsync(Guid assessmentId, object @event, CancellationToken cancellationToken)
  {
    if (assessmentId == Guid.Empty)
    {
      throw new ArgumentException("AssessmentId must be provided.", nameof(assessmentId));
    }

    var stream = await _session.Events.FetchStreamStateAsync(assessmentId, cancellationToken);

    if (stream is null)
    {
      _session.Events.StartStream<AssessmentUserAssignment>(assessmentId, @event);
    }
    else
    {
      _session.Events.Append(assessmentId, @event);
    }

    await _session.SaveChangesAsync(cancellationToken);

    return assessmentId;
  }
}
