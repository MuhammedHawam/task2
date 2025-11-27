using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.Entities;
using Marten;

namespace GAIA.Infra.Repositories
{
  public class AssessmentRepository : IAssessmentRepository
  {
    private readonly IDocumentSession _session;

    public AssessmentRepository(IDocumentSession session)
    {
      _session = session;
    }

    public async Task AddAsync(Assessment assessment, CancellationToken cancellationToken = default)
    {
      _session.Store(assessment);
      await _session.SaveChangesAsync(cancellationToken);
    }

    public async Task<Assessment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      return await _session.LoadAsync<Assessment>(id, cancellationToken);
    }

    public async Task UpdateAsync(Assessment assessment, CancellationToken cancellationToken = default)
    {
      _session.Store(assessment);
      await _session.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
      _session.Delete<Assessment>(id);
      await _session.SaveChangesAsync(cancellationToken);
    }
  }
}
