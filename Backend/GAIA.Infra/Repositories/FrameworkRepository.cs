using GAIA.Core.Framework.Interfaces;
using GAIA.Domain.Framework.Entities;
using Marten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAIA.Infra.Repositories
{
  public class FrameworkRepository : IFrameworkRepository
  {
    private readonly IDocumentSession _session;

    public FrameworkRepository(IDocumentSession session)
    {
      _session = session;
    }

    public async Task AddAsync(Framework framework)
    {
      _session.Store(framework);
      await _session.SaveChangesAsync();
    }

    public async Task<Framework> GetByIdAsync(Guid id)
    {
      return await _session.LoadAsync<Framework>(id);
    }

    public async Task UpdateAsync(Framework framework)
    {
      _session.Store(framework);
      await _session.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
      _session.Delete<Framework>(id);
      await _session.SaveChangesAsync();
    }
  }
}
