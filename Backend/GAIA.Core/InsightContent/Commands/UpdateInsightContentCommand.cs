using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.InsightContent.DomainEvents;
using MediatR;
using Marten;

namespace GAIA.Core.InsightContent.Commands;

public record UpdateInsightContentCommand(Guid AssessmentId, Guid InsightId, string Content)
  : IRequest<UpdateInsightContentResult?>;

public record UpdateInsightContentResult(Guid AssessmentId, Guid InsightId, string Content);

public class UpdateInsightContentCommandHandler
  : IRequestHandler<UpdateInsightContentCommand, UpdateInsightContentResult?>
{
  private readonly IQuerySession _querySession;
  private readonly IAssessmentEventWriter _eventWriter;

  public UpdateInsightContentCommandHandler(
    IQuerySession querySession,
    IAssessmentEventWriter eventWriter)
  {
    _querySession = querySession;
    _eventWriter = eventWriter;
  }

  public async Task<UpdateInsightContentResult?> Handle(
    UpdateInsightContentCommand request,
    CancellationToken cancellationToken)
  {
    var streamState = await _querySession.Events.FetchStreamStateAsync(request.AssessmentId, cancellationToken);
    if (streamState is null)
    {
      return null;
    }

    var sanitizedContent = (request.Content ?? string.Empty).Trim();
    if (string.IsNullOrWhiteSpace(sanitizedContent))
    {
      throw new InvalidOperationException("Insight content cannot be empty.");
    }
    var timestamp = DateTime.UtcNow;

    var userUpdatedEvent = new UserUpdatedInsightEvent
    {
      InsightId = request.InsightId,
      Content = sanitizedContent,
      Timestamp = timestamp
    };

    await _eventWriter.AppendAsync(request.AssessmentId, userUpdatedEvent, cancellationToken);

    return new UpdateInsightContentResult(
      request.AssessmentId,
      request.InsightId,
      sanitizedContent);
  }
}

