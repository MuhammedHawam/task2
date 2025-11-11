using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

        var frameworks = new List<Framework>
        {
          new Framework { Id = frameworkZetaId, Title = "Zeta Framework" },
          new Framework { Id = frameworkAlphaId, Title = "Alpha Framework" }
        };

        var depths = new List<AssessmentDepth>
        {
          new AssessmentDepth { Id = depthAlphaBId, FrameworkId = frameworkAlphaId, Depth = 2, Name = "B Depth" },
          new AssessmentDepth { Id = depthAlphaAId, FrameworkId = frameworkAlphaId, Depth = 1, Name = "A Depth" },
          new AssessmentDepth { Id = depthZetaId, FrameworkId = frameworkZetaId, Depth = 1, Name = "Research" }
        };

        var scorings = new List<AssessmentScoring>
        {
          new AssessmentScoring { Id = Guid.NewGuid(), AssessmentDepthId = depthAlphaAId, Name = "Silver" },
          new AssessmentScoring { Id = Guid.NewGuid(), AssessmentDepthId = depthAlphaAId, Name = "Bronze" },
          new AssessmentScoring { Id = Guid.NewGuid(), AssessmentDepthId = depthZetaId, Name = "Gold" }
        };

        var service = new AssessmentConfigurationService(frameworks, depths, scorings);

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
                Assert.Equal(1, depth.Depth);
                Assert.Collection(depth.AssessmentScorings,
                  scoring => Assert.Equal("Bronze", scoring.Name),
                  scoring => Assert.Equal("Silver", scoring.Name));
              },
              depth =>
              {
                Assert.Equal("B Depth", depth.Name);
                Assert.Equal(2, depth.Depth);
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
            Assert.Equal(1, depth.Depth);

            Assert.Single(depth.AssessmentScorings);
            Assert.Equal("Gold", depth.AssessmentScorings[0].Name);
          });
      }

    }
}
