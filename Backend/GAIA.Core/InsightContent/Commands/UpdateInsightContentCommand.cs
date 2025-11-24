using GAIA.Core.Assessment.Interfaces;
using GAIA.Core.InsightContent.Interfaces;
using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.InsightContent.Entities;
using MediatR;

namespace GAIA.Core.InsightContent.Commands;

public record UpdateInsightContentCommand(Guid AssessmentId, Guid InsightId, string Content)
  : IRequest<UpdateInsightContentResult?>;

public record UpdateInsightContentResult(Guid AssessmentId, Guid InsightId, string Content, bool CreatedNew);

public class UpdateInsightContentCommandHandler
  : IRequestHandler<UpdateInsightContentCommand, UpdateInsightContentResult?>
{
  private readonly IAssessmentRepository _assessmentRepository;
  private readonly IInsightContentRepository _insightContentRepository;
  private readonly IAssessmentEventWriter _eventWriter;

  public UpdateInsightContentCommandHandler(
    IAssessmentRepository assessmentRepository,
    IInsightContentRepository insightContentRepository,
    IAssessmentEventWriter eventWriter)
  {
    _assessmentRepository = assessmentRepository;
    _insightContentRepository = insightContentRepository;
    _eventWriter = eventWriter;
  }

  public async Task<UpdateInsightContentResult?> Handle(
    UpdateInsightContentCommand request,
    CancellationToken cancellationToken)
  {
    var assessment = await _assessmentRepository.GetByIdAsync(request.AssessmentId);
    if (assessment is null)
    {
      return null;
    }

    var sanitizedContent = (request.Content ?? string.Empty).Trim();
    if (string.IsNullOrWhiteSpace(sanitizedContent))
    {
      throw new InvalidOperationException("Insight content cannot be empty.");
    }
    var insight = await _insightContentRepository.GetByIdAsync(request.InsightId);

    var createdNew = insight is null;
    var timestamp = DateTime.UtcNow;

    if (createdNew)
    {
      insight = new InsightContent
      {
        Id = request.InsightId,
        AssessmentId = request.AssessmentId,
        CreatedAt = timestamp,
        CreatedBy = Guid.Empty,
        Content = sanitizedContent
      };

      await _insightContentRepository.AddAsync(insight);
    }
    else
    {
      insight.Content = sanitizedContent;
      await _insightContentRepository.UpdateAsync(insight);
    }

    var userUpdatedEvent = new UserUpdatedInsightEvent
    {
      AssessmentId = request.AssessmentId,
      InsightId = request.InsightId,
      Content = sanitizedContent,
      OccurredAt = timestamp
    };

    await _eventWriter.AppendAsync(request.AssessmentId, userUpdatedEvent, cancellationToken);

    return new UpdateInsightContentResult(
      request.AssessmentId,
      request.InsightId,
      sanitizedContent,
      createdNew);
  }
}
