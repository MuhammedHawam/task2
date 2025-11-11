using GAIA.Core.Services.Configuration;
using GAIA.Domain.Assessment.Entities;
using Xunit;

namespace GAIA.Tests.Domain.Assessment
{
  public class AssessmentTests
  {
    [Fact]
    public void Apply_Sets_All_Fields_From_Event()
    {
      var id = Guid.NewGuid();
      var createdBy = Guid.NewGuid();
      var frameworkId = Guid.NewGuid();
      var assessmentDepthId = Guid.NewGuid();
      var assessmentScoringId = Guid.NewGuid();

      var @event = new GAIA.Domain.Assessment.DomainEvents.AssessmentCreated
      {
        Id = id,
        Title = "Risk Assessment",
        Description = "Assess risk across systems",
        CreatedAt = DateTime.UtcNow,
        CreatedBy = createdBy,
        FrameworkId = frameworkId,
        AssessmentDepthId = assessmentDepthId,
        AssessmentScoringId = assessmentScoringId
      };

      var assessment = new GAIA.Domain.Assessment.Entities.Assessment();

      assessment.Apply(@event);

      Assert.Equal(id, assessment.Id);
      Assert.Equal(@event.Title, assessment.Title);
      Assert.Equal(@event.Description, assessment.Description);
      Assert.Equal(@event.CreatedAt, assessment.CreatedAt);
      Assert.Equal(@event.CreatedBy, assessment.CreatedBy);
      Assert.Equal(@event.FrameworkId, assessment.FrameworkId);
      Assert.Equal(@event.AssessmentDepthId, assessment.AssessmentDepthId);
      Assert.Equal(@event.AssessmentScoringId, assessment.AssessmentScoringId);
    }

    [Fact]
    public async Task GetOptionsAsync_MapsHierarchyWithOrderedChildren()
    {
      var frameworkAlphaId = Guid.NewGuid();
      var frameworkZetaId = Guid.NewGuid();

      var depthAlphaAId = Guid.NewGuid();
      var depthAlphaBId = Guid.NewGuid();
      var depthZetaId = Guid.NewGuid();

      var frameworks = new List<GAIA.Domain.Framework.Entities.Framework>
      {
        new GAIA.Domain.Framework.Entities.Framework { Id = frameworkZetaId, Title = "Zeta Framework" },
        new GAIA.Domain.Framework.Entities.Framework { Id = frameworkAlphaId, Title = "Alpha Framework" }
      };

      var depths = new List<AssessmentDepth>
      {
        new AssessmentDepth { Id = depthAlphaBId, FrameworkId = frameworkAlphaId, Name = "B Depth", Depth = 2 },
        new AssessmentDepth { Id = depthAlphaAId, FrameworkId = frameworkAlphaId, Name = "A Depth", Depth = 1 },
        new AssessmentDepth { Id = depthZetaId, FrameworkId = frameworkZetaId, Name = "Research", Depth = 1 }
      };

      var scorings = new List<AssessmentScoring>
      {
        new AssessmentScoring { Id = Guid.NewGuid(), FrameworkId = frameworkAlphaId, Name = "Silver" },
        new AssessmentScoring { Id = Guid.NewGuid(), FrameworkId = frameworkAlphaId, Name = "Bronze" },
        new AssessmentScoring { Id = Guid.NewGuid(), FrameworkId = frameworkZetaId, Name = "Gold" }
      };

      var service = new AssessmentConfigurationService(frameworks, depths, scorings);

      var result = await service.GetOptionsAsync(CancellationToken.None);

        Assert.Collection(
          result.Frameworks,
          framework =>
          {
            Assert.Equal(frameworkAlphaId, framework.Id);
            Assert.Equal("Alpha Framework", framework.Name);

            Assert.Collection(
              framework.AssessmentDepths,
              depth =>
              {
                Assert.Equal("A Depth", depth.Name);
                Assert.Equal(1, depth.Depth);
              },
              depth =>
              {
                Assert.Equal("B Depth", depth.Name);
                Assert.Equal(2, depth.Depth);
              });

            Assert.Collection(
              framework.AssessmentScorings,
              scoring => Assert.Equal("Bronze", scoring.Name),
              scoring => Assert.Equal("Silver", scoring.Name));
          },
          framework =>
          {
            Assert.Equal(frameworkZetaId, framework.Id);
            Assert.Equal("Zeta Framework", framework.Name);

            Assert.Single(framework.AssessmentDepths);
            var depth = framework.AssessmentDepths[0];
            Assert.Equal("Research", depth.Name);
            Assert.Equal(1, depth.Depth);

            Assert.Single(framework.AssessmentScorings);
            Assert.Equal("Gold", framework.AssessmentScorings[0].Name);
          });
    }
  }
}


