using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.Entities;
using MediatR;

namespace GAIA.Core.Assessment.Queries.FirstStep;

public record GetAssessmentFirstStepsQuery : IRequest<IReadOnlyList<AssessmentFirstStep>>;

public class GetAssessmentFirstStepsQueryHandler
  : IRequestHandler<GetAssessmentFirstStepsQuery, IReadOnlyList<AssessmentFirstStep>>
{
  private readonly IAssessmentFirstStepRepository _repository;

  public GetAssessmentFirstStepsQueryHandler(IAssessmentFirstStepRepository repository)
  {
    _repository = repository;
  }

  public async Task<IReadOnlyList<AssessmentFirstStep>> Handle(
    GetAssessmentFirstStepsQuery request,
    CancellationToken cancellationToken)
  {
    return await _repository.ListAsync(cancellationToken);
  }
}
