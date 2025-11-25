using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.DomainEvents;
using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment;

public class UpdateAssessmentCommandHandler
  : IRequestHandler<UpdateAssessmentCommand, Domain.Assessment.Entities.Assessment?>
{
  private readonly IAssessmentRepository _repository;
  private readonly IAssessmentEventWriter _eventWriter;

  public UpdateAssessmentCommandHandler(
    IAssessmentRepository repository,
    IAssessmentEventWriter eventWriter)
  {
    _repository = repository;
    _eventWriter = eventWriter;
  }

  public async Task<Domain.Assessment.Entities.Assessment?> Handle(
    UpdateAssessmentCommand request,
    CancellationToken cancellationToken)
  {
    var assessment = await _repository.GetByIdAsync(request.AssessmentId, cancellationToken);
    if (assessment is null)
    {
      return null;
    }

    var timestamp = DateTime.UtcNow;

    var updatedEvent = new AssessmentUpdated
    {
      Id = request.AssessmentId,
      Name = request.Name,
      StartDate = request.StartDate,
      EndDate = request.EndDate,
      OrganizationId = request.OrganizationId,
      Organization = request.Organization,
      Language = request.Language,
      UpdatedAt = timestamp
    };

    await _eventWriter.AppendAsync(request.AssessmentId, updatedEvent, cancellationToken);

    assessment.Name = request.Name;
    assessment.StartDate = request.StartDate;
    assessment.EndDate = request.EndDate;
    assessment.OrganizationId = request.OrganizationId;
    assessment.Organization = request.Organization;
    assessment.Language = request.Language;
    assessment.UpdatedAt = timestamp;

    return assessment;
  }
}
