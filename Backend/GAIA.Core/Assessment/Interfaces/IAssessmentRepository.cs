namespace GAIA.Core.Assessment.Interfaces;

public interface IAssessmentRepository
{
  Task AddAsync(Domain.Assessment.Entities.Assessment assessment);
  Task<Domain.Assessment.Entities.Assessment?> GetByIdAsync(Guid id);
  Task UpdateAsync(Domain.Assessment.Entities.Assessment assessment);
  Task DeleteAsync(Guid id);
}