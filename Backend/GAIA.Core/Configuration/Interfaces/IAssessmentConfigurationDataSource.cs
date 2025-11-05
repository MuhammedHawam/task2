using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GAIA.Domain.Assessment.Entities;
using GAIA.Domain.Framework.Entities;

namespace GAIA.Core.Configuration.Interfaces
{
  public interface IAssessmentConfigurationDataSource
  {
    Task<IReadOnlyList<Framework>> GetFrameworksAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AssessmentDepth>> GetAssessmentDepthsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AssessmentScoring>> GetAssessmentScoringsAsync(CancellationToken cancellationToken);
  }
}
