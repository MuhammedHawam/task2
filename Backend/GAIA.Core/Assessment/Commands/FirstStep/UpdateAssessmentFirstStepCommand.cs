using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Commands.FirstStep;

public record UpdateAssessmentFirstStepCommand(
  Guid AssessmentId,
  string Name,
  DateTime StartDate,
  DateTime EndDate,
  string Organization,
  string Language
) : IRequest<AssessmentFirstStep?>;

public class UpdateAssessmentFirstStepCommandHandler
  : IRequestHandler<UpdateAssessmentFirstStepCommand, AssessmentFirstStep?>
{
  private readonly IAssessmentFirstStepRepository _repository;
  private readonly IAssessmentFirstStepEventWriter _eventWriter;

  public UpdateAssessmentFirstStepCommandHandler(
    IAssessmentFirstStepRepository repository,
    IAssessmentFirstStepEventWriter eventWriter)
  {
    _repository = repository;
    _eventWriter = eventWriter;
  }

  public async Task<AssessmentFirstStep?> Handle(
    UpdateAssessmentFirstStepCommand request,
    CancellationToken cancellationToken)
  {
    var assessment = await _repository.GetByIdAsync(request.AssessmentId, cancellationToken);
    if (assessment is null)
    {
      return null;
    }

    var timestamp = DateTime.UtcNow;

    var updatedEvent = new AssessmentFirstStepUpdated
    {
      Id = request.AssessmentId,
      Name = request.Name,
      StartDate = request.StartDate,
      EndDate = request.EndDate,
      Organization = request.Organization,
      Language = request.Language,
      UpdatedAt = timestamp
    };

    await _eventWriter.AppendAsync(request.AssessmentId, updatedEvent, cancellationToken);

    assessment.Name = request.Name;
    assessment.StartDate = request.StartDate;
    assessment.EndDate = request.EndDate;
    assessment.Organization = request.Organization;
    assessment.Language = request.Language;
    assessment.UpdatedAt = timestamp;

    return assessment;
  }
}
