namespace GAIA.Core.Assessment.Interfaces;

public interface IAssessmentDetailsRepository
{
  Task AddAsync(Domain.Assessment.Entities.AssessmentDetails assessment);
  Task<Domain.Assessment.Entities.AssessmentDetails?> GetByIdAsync(Guid id);
  Task UpdateAsync(Domain.Assessment.Entities.AssessmentDetails assessment);
  Task DeleteAsync(Guid id);
}
