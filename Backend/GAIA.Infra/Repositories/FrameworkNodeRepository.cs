using GAIA.Core.FrameworkNode.Interfaces;
using GAIA.Domain.Framework.Entities;
using Marten;

namespace GAIA.Infra.Repositories
{
  public class FrameworkNodeRepository : IFrameworkNodeRepository
  {
    private readonly IDocumentSession _session;

    public FrameworkNodeRepository(IDocumentSession session)
    {
      _session = session;
    }

    public async Task AddAsync(FrameworkNode frameworkNode)
    {
      _session.Store(frameworkNode);
      await _session.SaveChangesAsync();
    }

    public async Task<FrameworkNode> GetByIdAsync(Guid id)
    {
      return await _session.LoadAsync<FrameworkNode>(id);
    }

    public async Task UpdateAsync(FrameworkNode frameworkNode)
    {
      _session.Store(frameworkNode);
      await _session.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
      _session.Delete<FrameworkNode>(id);
      await _session.SaveChangesAsync();
    }
  }
}
