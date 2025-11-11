using GAIA.Core.Assessment.Models;

namespace GAIA.Core.Assessment.Interfaces
{
  public interface IAssessmentConfigurationService
  {
    Task<AssessmentConfigurationOptions> GetOptionsAsync(CancellationToken cancellationToken);
  }
}
