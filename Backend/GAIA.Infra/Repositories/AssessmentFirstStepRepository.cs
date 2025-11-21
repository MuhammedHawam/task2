using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.Entities;
using Marten;

namespace GAIA.Infra.Repositories
{
  public class AssessmentFirstStepRepository : IAssessmentFirstStepRepository
  {
    private readonly IQuerySession _querySession;

    public AssessmentFirstStepRepository(IQuerySession querySession)
    {
      _querySession = querySession;
    }

    public Task<AssessmentFirstStep?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
      return _querySession.LoadAsync<AssessmentFirstStep>(id, cancellationToken);
    }
  }
}
