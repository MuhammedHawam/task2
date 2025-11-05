using GAIA.Core.Interfaces.Assessment;
using GAIA.Domain.DomainEvents;
using MediatR;

namespace GAIA.Core.Commands.Assessment;

public class CreateAssessmentCommandHandler : IRequestHandler<CreateAssessmentCommand, CreateAssessmentResult>
{
  private readonly IAssessmentEventWriter _writer;

  public CreateAssessmentCommandHandler(IAssessmentEventWriter writer)
  {
    _writer = writer;
  }

  public async Task<CreateAssessmentResult> Handle(CreateAssessmentCommand request, CancellationToken cancellationToken)
  {
    var assessmentId = Guid.NewGuid();
    var createdEvent = new AssessmentCreated
    {
      Id = assessmentId,
      Title = request.Title,
      Description = request.Description,
      CreatedAt = DateTime.UtcNow,
      CreatedBy = request.CreatedBy,
      FrameworkId = request.FrameworkId,
      AssessmentDepthId = request.AssessmentDepthId,
      AssessmentScoringId = request.AssessmentScoringId
    };

    var id = await _writer.CreateAsync(createdEvent, cancellationToken);
    return new CreateAssessmentResult(id);
  }
}
