namespace GAIA.Core.InsightContent.Interfaces
{
  public interface IInsightContentRepository
  {
    Task AddAsync(Domain.InsightContent.Entities.InsightContent insightContent);
    Task<Domain.InsightContent.Entities.InsightContent?> GetByIdAsync(Guid id);
    Task UpdateAsync(Domain.InsightContent.Entities.InsightContent insightContent);
    Task DeleteAsync(Guid id);
  }
}
