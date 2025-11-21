using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Commands.FirstStep;

public record CreateAssessmentFirstStepCommand(
  string Name,
  DateTime StartDate,
  DateTime EndDate,
  string Organization,
  string Language
) : IRequest<AssessmentFirstStep>;

public class CreateAssessmentFirstStepCommandHandler
  : IRequestHandler<CreateAssessmentFirstStepCommand, AssessmentFirstStep>
{
  private readonly IAssessmentFirstStepEventWriter _eventWriter;

  public CreateAssessmentFirstStepCommandHandler(IAssessmentFirstStepEventWriter eventWriter)
  {
    _eventWriter = eventWriter;
  }

  public async Task<AssessmentFirstStep> Handle(
    CreateAssessmentFirstStepCommand request,
    CancellationToken cancellationToken)
  {
    var assessmentId = Guid.NewGuid();
    var timestamp = DateTime.UtcNow;

    var createdEvent = new AssessmentFirstStepCreated
    {
      Id = assessmentId,
      Name = request.Name,
      StartDate = request.StartDate,
      EndDate = request.EndDate,
      Organization = request.Organization,
      Language = request.Language,
      CreatedAt = timestamp
    };

    await _eventWriter.CreateAsync(createdEvent, cancellationToken);

    return new AssessmentFirstStep
    {
      Id = assessmentId,
      Name = request.Name,
      StartDate = request.StartDate,
      EndDate = request.EndDate,
      Organization = request.Organization,
      Language = request.Language,
      CreatedAt = timestamp
    };
  }
}
