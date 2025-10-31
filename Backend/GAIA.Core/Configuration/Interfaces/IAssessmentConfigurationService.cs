using System.Threading;
using System.Threading.Tasks;
using GAIA.Core.Configuration.Models;

namespace GAIA.Core.Configuration.Interfaces
{
  public interface IAssessmentConfigurationService
  {
    Task<AssessmentConfigurationOptions> GetOptionsAsync(CancellationToken cancellationToken);
  }
}
