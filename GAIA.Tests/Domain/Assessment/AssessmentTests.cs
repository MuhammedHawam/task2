using GAIA.Api.Contracts.Assessment;
using GAIA.Api.Controllers;
using GAIA.Core.Assessment.Queries;
using GAIA.Core.InsightContent.Commands;
using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.Assessment.Entities;
using GAIA.Domain.Framework;
using GAIA.Tests.Mocks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GAIA.Tests.Domain.Assessment;

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

    var @event = new AssessmentCreated
    {
      Id = id,
      Title = "Risk Assessment",
      Description = "Assess risk across systems",
      CreatedAt = DateTime.UtcNow,
      CreatedBy = createdBy,
      FrameworkId = frameworkId,
      AssessmentDepthId = assessmentDepthId,
      AssessmentScoringId = assessmentScoringId,
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

    var frameworks = new List<Framework>
    {
      new Framework
      {
        Id = frameworkZetaId,
        Title = "Zeta Framework",
        Description = "Description for Zeta",
        CreatedAt = default,
        CreatedBy = "Test",
        Root = new FrameworkNode { Id = Guid.NewGuid(), Content = null },
        Depths =
        [
          new FrameworkDepth { Id = depthAlphaBId, Name = "B Depth", Depth = 2 },
          new FrameworkDepth { Id = depthAlphaAId, Name = "A Depth", Depth = 1 },
        ],
        Scorings =
        [
          new Scoring { Id = Guid.NewGuid(), Name = "Silver" },
          new Scoring { Id = Guid.NewGuid(), Name = "Bronze" },
        ],
      },
      new Framework
      {
        Id = frameworkAlphaId,
        Title = "Alpha Framework",
        Description = "Description for Alpha",
        CreatedAt = default,
        CreatedBy = "Test",
        Root = new FrameworkNode { Id = Guid.NewGuid(), Content = null },
        Depths = [new FrameworkDepth { Id = depthZetaId, Name = "Research", Depth = 1 }],
        Scorings = [new Scoring { Id = Guid.NewGuid(), Name = "Gold" }],
      },
    };

    var repo = new MockRepository<Framework>(frameworks);
    var service = new FrameworkService(repo);

    var result = await service.ListFrameworksWithOptions();

    Assert.Collection(
      result,
      framework =>
      {
        Assert.Equal(frameworkAlphaId, framework.Id);
        Assert.Equal("Alpha Framework", framework.Name);

        Assert.Collection(
          framework.AssessmentDepths, depth =>
          {
            Assert.Equal((string?)"A Depth", (string?)depth.Name);
            Assert.Equal(1, depth.Depth);
          }, depth =>
          {
            Assert.Equal((string?)"B Depth", (string?)depth.Name);
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
        var depth = framework.AssessmentDepths.First();
        Assert.Equal("Research", depth.Name);
        Assert.Equal(1, depth.Depth);

        Assert.Single(framework.AssessmentScorings);
        Assert.Equal("Gold", framework.AssessmentScorings.First().Name);
      });
  }

  [Fact]
  public async Task GetAll_ReturnsAssessmentsWithDepthAndScoring()
  {
    // Arrange
    var assessmentId = Guid.NewGuid();
    var depthId = Guid.NewGuid();
    var scoringId = Guid.NewGuid();

    var assessment = new GAIA.Domain.Assessment.Entities.Assessment
    {
      Id = assessmentId,
      Title = "Security Review",
      Description = "Annual security assessment",
      CreatedAt = DateTime.UtcNow,
      CreatedBy = Guid.NewGuid(),
      FrameworkId = Guid.NewGuid(),
      AssessmentDepthId = depthId,
      AssessmentScoringId = scoringId,
    };

    var depth = new AssessmentDepth
    {
      Id = depthId,
      FrameworkId = assessment.FrameworkId,
      Name = "Advanced",
      Depth = 2,
    };

    var scoring = new AssessmentScoring
    {
      Id = scoringId,
      FrameworkId = assessment.FrameworkId,
      Name = "Weighted",
      Description = "Weighted scoring model",
    };

    var details = new AssessmentDetails(assessment, depth, scoring);
    var controller = CreateController((request, _) => request switch
    {
      GetAssessmentsQuery => new List<AssessmentDetails> { details },
      _ => throw new InvalidOperationException("Unexpected request"),
    });

    // Act
    var result = await controller.GetAll(CancellationToken.None);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<List<AssessmentResponse>>(okResult.Value);
    var item = Assert.Single(response);

    Assert.Equal(assessmentId, item.Id);
    Assert.Equal("Security Review", item.Title);
    Assert.NotNull(item.Depth);
    Assert.Equal(depthId, item.Depth!.Id);
    Assert.NotNull(item.Scoring);
    Assert.Equal(scoringId, item.Scoring!.Id);
  }

  [Fact]
  public async Task GetById_ReturnsAssessmentWithDepthAndScoring()
  {
    // Arrange
    var assessmentId = Guid.NewGuid();
    var depthId = Guid.NewGuid();
    var scoringId = Guid.NewGuid();

    var assessment = new GAIA.Domain.Assessment.Entities.Assessment
    {
      Id = assessmentId,
      Title = "Privacy Review",
      Description = "Privacy compliance assessment",
      CreatedAt = DateTime.UtcNow,
      CreatedBy = Guid.NewGuid(),
      FrameworkId = Guid.NewGuid(),
      AssessmentDepthId = depthId,
      AssessmentScoringId = scoringId,
    };

    var depth = new AssessmentDepth
    {
      Id = depthId,
      FrameworkId = assessment.FrameworkId,
      Name = "Baseline",
      Depth = 1,
    };

    var scoring = new AssessmentScoring
    {
      Id = scoringId,
      FrameworkId = assessment.FrameworkId,
      Name = "Simple",
      Description = "Simple scoring",
    };

    var details = new AssessmentDetails(assessment, depth, scoring);

    var controller = CreateController((request, _) => request switch
    {
      GetAssessmentByIdQuery q when q.AssessmentId == assessmentId => details,
      _ => throw new InvalidOperationException("Unexpected request"),
    });

    // Act
    var result = await controller.GetById(assessmentId, CancellationToken.None);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<AssessmentResponse>(okResult.Value);

    Assert.Equal(assessmentId, response.Id);
    Assert.Equal("Privacy Review", response.Title);
    Assert.NotNull(response.Depth);
    Assert.Equal(depthId, response.Depth!.Id);
    Assert.NotNull(response.Scoring);
    Assert.Equal(scoringId, response.Scoring!.Id);
  }

  [Fact]
  public async Task GetById_ReturnsNotFoundWhenAssessmentMissing()
  {
    // Arrange
    var controller = CreateController((request, _) => request switch
    {
      GetAssessmentByIdQuery => null,
      _ => throw new InvalidOperationException("Unexpected request"),
    });

    // Act
    var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

    // Assert
    Assert.IsType<NotFoundResult>(result.Result);
  }

  [Fact]
  public async Task UpdateInsightContent_ReturnsOk_WhenCommandSucceeds()
  {
    // Arrange
    var assessmentId = Guid.NewGuid();
    var insightId = Guid.NewGuid();
    var controller = CreateController((request, _) => request switch
    {
      UpdateInsightContentCommand => new UpdateInsightContentResult(assessmentId, insightId, "Updated content", false),
      _ => throw new InvalidOperationException("Unexpected request"),
    });

    // Act
    var result = await controller.UpdateInsightContent(
      assessmentId,
      insightId,
      new UpdateInsightContentRequest("Updated content"),
      CancellationToken.None);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<UpdateInsightContentResponse>(okResult.Value);
    Assert.Equal(assessmentId, response.AssessmentId);
    Assert.Equal(insightId, response.InsightId);
    Assert.False(response.CreatedNew);
  }

  [Fact]
  public async Task UpdateInsightContent_ReturnsNotFound_WhenCommandReturnsNull()
  {
    // Arrange
    var controller = CreateController((request, _) => request switch
    {
      UpdateInsightContentCommand => null,
      _ => throw new InvalidOperationException("Unexpected request"),
    });

    // Act
    var result = await controller.UpdateInsightContent(
      Guid.NewGuid(),
      Guid.NewGuid(),
      new UpdateInsightContentRequest("content"),
      CancellationToken.None);

    // Assert
    Assert.IsType<NotFoundResult>(result.Result);
  }

  private static AssessmentsController CreateController(Func<object, CancellationToken, object?> responseFactory)
  {
    var sender = new TestSender(responseFactory);
    var repo = new MockRepository<Framework>([]);
    var service = new FrameworkService(repo);
    return new AssessmentsController(sender, service);
  }

  private sealed class TestSender(Func<object, CancellationToken, object?> responseFactory) : ISender
  {
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
      var response = responseFactory(request, cancellationToken);
      return Task.FromResult(response is null ? default! : (TResponse)response);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest
    {
      responseFactory(request, cancellationToken);
      return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken)
    {
      return Task.FromResult(responseFactory(request, cancellationToken));
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request,
      CancellationToken cancellationToken)
    {
      throw new NotSupportedException();
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken)
    {
      throw new NotSupportedException();
    }
  }
}
