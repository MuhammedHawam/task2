namespace GAIA.Core.Framework.Interfaces
{
  public interface IFrameworkRepository
  {
    Task AddAsync(Domain.Framework.Entities.Framework framework);
    Task<Domain.Framework.Entities.Framework?> GetByIdAsync(Guid id);
    Task UpdateAsync(Domain.Framework.Entities.Framework framework);
    Task DeleteAsync(Guid id);
  }
}
