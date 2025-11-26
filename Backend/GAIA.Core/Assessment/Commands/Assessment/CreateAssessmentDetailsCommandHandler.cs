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
    if (request.AssessmentId == Guid.Empty)
    {
      throw new ArgumentException("AssessmentId must be provided.", nameof(request.AssessmentId));
    }

    var assessmentDetailsId = Guid.NewGuid();
    var createdEvent = new AssessmentDetailsCreated
    {
      Id = assessmentDetailsId,
      AssessmentId = request.AssessmentId,
      Title = request.Title,
      Description = request.Description,
      CreatedAt = DateTime.UtcNow,
      CreatedBy = request.CreatedBy,
      FrameworkId = request.FrameworkId,
      AssessmentDepthId = request.AssessmentDepthId,
      AssessmentScoringId = request.AssessmentScoringId
    };

    var createdAssessmentDetailsId = await _writer.CreateAsync(createdEvent, cancellationToken);
    return new CreateAssessmentDetailsResult(request.AssessmentId, createdAssessmentDetailsId);
  }
}
