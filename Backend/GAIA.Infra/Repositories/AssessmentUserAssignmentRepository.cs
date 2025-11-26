using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.Entities;
using Marten;

namespace GAIA.Infra.Repositories;

public class AssessmentUserAssignmentRepository : IAssessmentUserAssignmentRepository
{
  private readonly IDocumentSession _documentSession;
  private readonly IQuerySession _querySession;

  public AssessmentUserAssignmentRepository(IDocumentSession documentSession, IQuerySession querySession)
  {
    _documentSession = documentSession;
    _querySession = querySession;
  }

  public Task<AssessmentUserAssignment?> GetAsync(Guid assessmentId, CancellationToken cancellationToken)
  {
    return _querySession.LoadAsync<AssessmentUserAssignment>(assessmentId, cancellationToken);
  }

  public async Task SaveAsync(AssessmentUserAssignment assignment, CancellationToken cancellationToken)
  {
    _documentSession.Store(assignment);
    await _documentSession.SaveChangesAsync(cancellationToken);
  }
}
