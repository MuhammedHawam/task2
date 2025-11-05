using System;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GAIA.Core.Configuration.Interfaces;
using GAIA.Core.Services.Configuration;
using GAIA.Domain.Assessment.Entities;
using GAIA.Domain.Framework.Entities;
using Xunit;

namespace GAIA.Tests.Core.Configuration
{
  public class AssessmentConfigurationServiceTests
  {
    [Fact]
    public async Task GetOptionsAsync_MapsHierarchyWithOrderedChildren()
    {
      var frameworkAlphaId = Guid.NewGuid();
      var frameworkZetaId = Guid.NewGuid();

      var depthAlphaAId = Guid.NewGuid();
      var depthAlphaBId = Guid.NewGuid();
      var depthZetaId = Guid.NewGuid();

      var dataSource = new FakeConfigurationDataSource(
        new[]
        {
          new Framework { Id = frameworkZetaId, Title = "Zeta Framework" },
          new Framework { Id = frameworkAlphaId, Title = "Alpha Framework" }
        },
        new[]
        {
          new AssessmentDepth { Id = depthAlphaBId, FrameworkId = frameworkAlphaId, Name = "B Depth" },
          new AssessmentDepth { Id = depthAlphaAId, FrameworkId = frameworkAlphaId, Name = "A Depth" },
          new AssessmentDepth { Id = depthZetaId, FrameworkId = frameworkZetaId, Name = "Research" }
        },
        new[]
        {
          new AssessmentScoring { Id = Guid.NewGuid(), AssessmentDepthId = depthAlphaAId, Name = "Silver" },
          new AssessmentScoring { Id = Guid.NewGuid(), AssessmentDepthId = depthAlphaAId, Name = "Bronze" },
          new AssessmentScoring { Id = Guid.NewGuid(), AssessmentDepthId = depthZetaId, Name = "Gold" }
        });

      var service = new AssessmentConfigurationService(dataSource);

      var result = await service.GetOptionsAsync(CancellationToken.None);

      Assert.Collection(result.Frameworks,
        framework =>
        {
          Assert.Equal(frameworkAlphaId, framework.Id);
          Assert.Equal("Alpha Framework", framework.Name);

          Assert.Collection(framework.AssessmentDepths,
            depth =>
            {
              Assert.Equal("A Depth", depth.Name);
              Assert.Collection(depth.AssessmentScorings,
                scoring => Assert.Equal("Bronze", scoring.Name),
                scoring => Assert.Equal("Silver", scoring.Name));
            },
            depth =>
            {
              Assert.Equal("B Depth", depth.Name);
              Assert.Empty(depth.AssessmentScorings);
            });
        },
        framework =>
        {
          Assert.Equal(frameworkZetaId, framework.Id);
          Assert.Equal("Zeta Framework", framework.Name);

          Assert.Single(framework.AssessmentDepths);
          var depth = framework.AssessmentDepths[0];
          Assert.Equal("Research", depth.Name);

          Assert.Single(depth.AssessmentScorings);
          Assert.Equal("Gold", depth.AssessmentScorings[0].Name);
        });
    }

    private sealed class FakeConfigurationDataSource : IAssessmentConfigurationDataSource
    {
      private readonly IReadOnlyList<Framework> _frameworks;
      private readonly IReadOnlyList<AssessmentDepth> _assessmentDepths;
      private readonly IReadOnlyList<AssessmentScoring> _assessmentScorings;

      public FakeConfigurationDataSource(
        IReadOnlyList<Framework> frameworks,
        IReadOnlyList<AssessmentDepth> assessmentDepths,
        IReadOnlyList<AssessmentScoring> assessmentScorings)
      {
        _frameworks = frameworks;
        _assessmentDepths = assessmentDepths;
        _assessmentScorings = assessmentScorings;
      }

      public Task<IReadOnlyList<Framework>> GetFrameworksAsync(CancellationToken cancellationToken)
        => Task.FromResult(_frameworks);

      public Task<IReadOnlyList<AssessmentDepth>> GetAssessmentDepthsAsync(CancellationToken cancellationToken)
        => Task.FromResult(_assessmentDepths);

      public Task<IReadOnlyList<AssessmentScoring>> GetAssessmentScoringsAsync(CancellationToken cancellationToken)
        => Task.FromResult(_assessmentScorings);
    }
  }
}
