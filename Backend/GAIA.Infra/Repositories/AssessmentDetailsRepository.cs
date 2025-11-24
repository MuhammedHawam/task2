using GAIA.Core.Assessment.Interfaces;
using GAIA.Domain.Assessment.Entities;
using Marten;

namespace GAIA.Infra.Repositories
{
  public class AssessmentDetailsRepository : IAssessmentDetailsRepository
  {
    private readonly IDocumentSession _session;

    public AssessmentDetailsRepository(IDocumentSession session)
    {
      _session = session;
    }

    public async Task AddAsync(AssessmentDetails assessment)
    {
      _session.Store(assessment);
      await _session.SaveChangesAsync();
    }

    public async Task<AssessmentDetails> GetByIdAsync(Guid id)
    {
      return await _session.LoadAsync<AssessmentDetails>(id);
    }

    public async Task UpdateAsync(AssessmentDetails assessment)
    {
      _session.Store(assessment);
      await _session.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
      _session.Delete<AssessmentDetails>(id);
      await _session.SaveChangesAsync();
    }
  }
}
