using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GAIA.Core.Configuration.Interfaces;
using GAIA.Domain.Assessment.Entities;
using GAIA.Domain.Framework.Entities;
using Marten;
using Marten.Linq;

namespace GAIA.Infra.Configurations
{
  public class MartenAssessmentConfigurationDataSource : IAssessmentConfigurationDataSource
  {
    private readonly IDocumentSession _session;

    public MartenAssessmentConfigurationDataSource(IDocumentSession session)
    {
      _session = session;
    }

    public async Task<IReadOnlyList<Framework>> GetFrameworksAsync(CancellationToken cancellationToken)
      => await _session.Query<Framework>().ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<AssessmentDepth>> GetAssessmentDepthsAsync(CancellationToken cancellationToken)
      => await _session.Query<AssessmentDepth>().ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<AssessmentScoring>> GetAssessmentScoringsAsync(CancellationToken cancellationToken)
      => await _session.Query<AssessmentScoring>().ToListAsync(cancellationToken).ConfigureAwait(false);
  }
}
