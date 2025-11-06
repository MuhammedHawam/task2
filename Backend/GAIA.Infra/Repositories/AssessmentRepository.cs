using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.Entities;
using Marten;
using Marten.Linq;

namespace GAIA.Infra.Repositories
{
  public class AssessmentRepository : IAssessmentRepository
  {
    private readonly IDocumentSession _session;

    public AssessmentRepository(IDocumentSession session)
    {
      _session = session;
    }

    public async Task AddAsync(Assessment assessment)
    {
      _session.Store(assessment);
      await _session.SaveChangesAsync();
    }

    public async Task<Assessment?> GetByIdAsync(Guid id)
    {
      return await _session.LoadAsync<Assessment>(id);
    }

    public async Task UpdateAsync(Assessment assessment)
    {
      _session.Store(assessment);
      await _session.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
      _session.Delete<Assessment>(id);
      await _session.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Assessment>> ListAsync(CancellationToken cancellationToken = default)
    {
      return await _session.Query<Assessment>()
        .OrderByDescending(assessment => assessment.CreatedAt)
        .ToListAsync(cancellationToken);
    }
  }
}
