using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.Entities;
using Marten;

namespace GAIA.Infra.Repositories;

public class AssessmentRepository : IAssessmentRepository
{
  private readonly IQuerySession _querySession;

  public AssessmentRepository(IQuerySession querySession)
  {
    _querySession = querySession;
  }

  public Task<Assessment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
  {
    return _querySession.LoadAsync<Assessment>(id, cancellationToken);
  }
  public async Task<IReadOnlyList<Assessment>> ListAsync(CancellationToken cancellationToken)
  {
    return await _querySession.Query<Assessment>().ToListAsync(cancellationToken);
  }
}
