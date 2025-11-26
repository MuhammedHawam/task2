using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.Entities;
using Marten;

namespace GAIA.Infra.Repositories;

public class AssessmentUserAssignmentRepository : IAssessmentUserAssignmentRepository
{
  private readonly IQuerySession _querySession;

  public AssessmentUserAssignmentRepository(IQuerySession querySession)
  {
    _querySession = querySession;
  }

  public Task<AssessmentUserAssignment?> GetAsync(Guid assessmentId, CancellationToken cancellationToken)
  {
    return _querySession.LoadAsync<AssessmentUserAssignment>(assessmentId, cancellationToken);
  }
}
