namespace GAIA.Core.FrameworkNode.Interfaces
{
  public interface IFrameworkNodeRepository
  {
    Task AddAsync(Domain.Framework.Entities.FrameworkNode node);
    Task<Domain.Framework.Entities.FrameworkNode?> GetByIdAsync(Guid id);
    Task UpdateAsync(Domain.Framework.Entities.FrameworkNode node);
    Task DeleteAsync(Guid id);
  }
}
