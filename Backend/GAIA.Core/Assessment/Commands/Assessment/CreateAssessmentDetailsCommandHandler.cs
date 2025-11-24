using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.DomainEvents;
using MediatR;

namespace GAIA.Core.Assessment.Commands.Assessment;

public class CreateAssessmentDetailsCommandHandler : IRequestHandler<CreateAssessmentDetailsCommand, CreateAssessmentDetailsResult>
{
  private readonly IAssessmentDetailsEventWriter _writer;

  public CreateAssessmentDetailsCommandHandler(IAssessmentDetailsEventWriter writer)
  {
    _writer = writer;
  }

  public async Task<CreateAssessmentDetailsResult> Handle(CreateAssessmentDetailsCommand request, CancellationToken cancellationToken)
  {
    var assessmentId = Guid.NewGuid();
    var createdEvent = new AssessmentDetailsCreated
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
    return new CreateAssessmentDetailsResult(id);
  }
}
