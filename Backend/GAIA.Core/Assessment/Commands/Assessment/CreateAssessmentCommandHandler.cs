using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.DomainEvents;
using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment;

public class CreateAssessmentCommandHandler
  : IRequestHandler<CreateAssessmentCommand, Domain.Assessment.Entities.Assessment>
{
  private readonly IAssessmentEventWriter _eventWriter;

  public CreateAssessmentCommandHandler(IAssessmentEventWriter eventWriter)
  {
    _eventWriter = eventWriter;
  }

  public async Task<Domain.Assessment.Entities.Assessment> Handle(
    CreateAssessmentCommand request,
    CancellationToken cancellationToken)
  {
    var assessmentId = Guid.NewGuid();
    var timestamp = DateTime.UtcNow;

    var createdEvent = new AssessmentCreated
    {
      Id = assessmentId,
      Name = request.Name,
      StartDate = request.StartDate,
      EndDate = request.EndDate,
      OrganizationId = request.OrganizationId,
      Organization = request.Organization,
      Language = request.Language,
      CreatedAt = timestamp
    };

    await _eventWriter.CreateAsync(createdEvent, cancellationToken);

    return new Domain.Assessment.Entities.Assessment
    {
      Id = assessmentId,
      Name = request.Name,
      StartDate = request.StartDate,
      EndDate = request.EndDate,
      OrganizationId = request.OrganizationId,
      Organization = request.Organization,
      Language = request.Language,
      CreatedAt = timestamp
    };
  }
}

