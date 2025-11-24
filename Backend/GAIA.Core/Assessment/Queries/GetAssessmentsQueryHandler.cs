using GAIA.Core.Assessment.Interfaces;
using MediatR;

namespace GAIA.Core.Assessment.Queries;

public class GetAssessmentsQueryHandler
  : IRequestHandler<GetAssessmentsQuery, IReadOnlyList<Domain.Assessment.Entities.Assessment>>
{
  private readonly IAssessmentRepository _repository;

  public GetAssessmentsQueryHandler(IAssessmentRepository repository)
  {
    _repository = repository;
  }

  public async Task<IReadOnlyList<Domain.Assessment.Entities.Assessment>> Handle(
    GetAssessmentsQuery request,
    CancellationToken cancellationToken)
  {
    return await _repository.ListAsync(cancellationToken);
  }
}

