using GAIA.Core.Assessment.Interfaces;
using MediatR;

namespace GAIA.Core.Assessment.Queries;

public class GetAssessmentByIdQueryHandler
  : IRequestHandler<GetAssessmentByIdQuery, Domain.Assessment.Entities.Assessment?>
{
  private readonly IAssessmentRepository _repository;

  public GetAssessmentByIdQueryHandler(IAssessmentRepository repository)
  {
    _repository = repository;
  }

  public Task<Domain.Assessment.Entities.Assessment?> Handle(
    GetAssessmentByIdQuery request,
    CancellationToken cancellationToken)
  {
    return _repository.GetByIdAsync(request.AssessmentId, cancellationToken);
  }
}
