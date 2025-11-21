using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Queries.FirstStep;

public record GetAssessmentFirstStepByIdQuery(Guid AssessmentId) : IRequest<AssessmentFirstStep?>;

public class GetAssessmentFirstStepByIdQueryHandler
  : IRequestHandler<GetAssessmentFirstStepByIdQuery, AssessmentFirstStep?>
{
  private readonly IAssessmentFirstStepRepository _repository;

  public GetAssessmentFirstStepByIdQueryHandler(IAssessmentFirstStepRepository repository)
  {
    _repository = repository;
  }

  public Task<AssessmentFirstStep?> Handle(
    GetAssessmentFirstStepByIdQuery request,
    CancellationToken cancellationToken)
  {
    return _repository.GetByIdAsync(request.AssessmentId, cancellationToken);
  }
}
