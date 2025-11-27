using GAIA.Core.InsightContent.Interfaces;
using GAIA.Domain.InsightContent.Entities;
using Marten;

namespace GAIA.Infra.Repositories
{
  public class InsightContentRepository : IInsightContentRepository
  {
    private readonly IDocumentSession _session;

    public InsightContentRepository(IDocumentSession session)
    {
      _session = session;
    }

    public async Task AddAsync(InsightContent insightContent)
    {
      _session.Store(insightContent);
      await _session.SaveChangesAsync();
    }

    public async Task<InsightContent?> GetByIdAsync(Guid id)
    {
      return await _session.LoadAsync<InsightContent>(id);
    }

    public async Task UpdateAsync(InsightContent insightContent)
    {
      _session.Store(insightContent);
      await _session.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
      _session.Delete<InsightContent>(id);
      await _session.SaveChangesAsync();
    }
  }
}
