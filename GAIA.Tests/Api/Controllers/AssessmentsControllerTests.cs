using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GAIA.Api.Contracts;
using GAIA.Api.Controllers;
using GAIA.Api.Mappers;
using GAIA.Core.Assessment.Queries;
using GAIA.Core.Configuration.Interfaces;
using GAIA.Core.Configuration.Models;
using GAIA.Domain.Assessment.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GAIA.Tests.Api.Controllers;

public class AssessmentsControllerTests
{
  [Fact]
  public async Task GetAll_ReturnsAssessmentsWithDepthAndScoring()
  {
    // Arrange
    var assessmentId = Guid.NewGuid();
    var depthId = Guid.NewGuid();
    var scoringId = Guid.NewGuid();

      var assessment = new Assessment
      {
        Id = assessmentId,
        Title = "Security Review",
        Description = "Annual security assessment",
        CreatedAt = DateTime.UtcNow,
        CreatedBy = Guid.NewGuid(),
        FrameworkId = Guid.NewGuid(),
        AssessmentDepthId = depthId,
        AssessmentScoringId = scoringId
      };

      var depth = new AssessmentDepth
      {
        Id = depthId,
        FrameworkId = assessment.FrameworkId,
        Name = "Advanced",
        Depth = 2
      };

      var scoring = new AssessmentScoring
      {
        Id = scoringId,
        FrameworkId = assessment.FrameworkId,
        Name = "Weighted",
        Description = "Weighted scoring model"
      };

    var details = new AssessmentDetails(assessment, depth, scoring);
    var controller = CreateController((request, _) => request switch
    {
      GetAssessmentsQuery => new List<AssessmentDetails> { details },
      _ => throw new InvalidOperationException("Unexpected request")
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

      var assessment = new Assessment
      {
        Id = assessmentId,
        Title = "Privacy Review",
        Description = "Privacy compliance assessment",
        CreatedAt = DateTime.UtcNow,
        CreatedBy = Guid.NewGuid(),
        FrameworkId = Guid.NewGuid(),
        AssessmentDepthId = depthId,
        AssessmentScoringId = scoringId
      };

      var depth = new AssessmentDepth
      {
        Id = depthId,
        FrameworkId = assessment.FrameworkId,
        Name = "Baseline",
        Depth = 1
      };

      var scoring = new AssessmentScoring
      {
        Id = scoringId,
        FrameworkId = assessment.FrameworkId,
        Name = "Simple",
        Description = "Simple scoring"
      };

    var details = new AssessmentDetails(assessment, depth, scoring);

    var controller = CreateController((request, _) => request switch
    {
      GetAssessmentByIdQuery q when q.AssessmentId == assessmentId => details,
      _ => throw new InvalidOperationException("Unexpected request")
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
      _ => throw new InvalidOperationException("Unexpected request")
    });

    // Act
    var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

    // Assert
    Assert.IsType<NotFoundResult>(result.Result);
  }

  private static AssessmentsController CreateController(Func<object, CancellationToken, object?> responseFactory)
  {
    var sender = new TestSender(responseFactory);
    var configurationService = new StubAssessmentConfigurationService();
    return new AssessmentsController(sender, configurationService);
  }

  private sealed class TestSender : ISender
  {
    private readonly Func<object, CancellationToken, object?> _responseFactory;

    public TestSender(Func<object, CancellationToken, object?> responseFactory)
    {
      _responseFactory = responseFactory;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
      var response = _responseFactory(request, cancellationToken);
      return Task.FromResult(response is null ? default! : (TResponse)response);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest
    {
      _responseFactory(request!, cancellationToken);
      return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken)
    {
      return Task.FromResult(_responseFactory(request, cancellationToken));
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken)
    {
      throw new NotSupportedException();
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken)
    {
      throw new NotSupportedException();
    }
  }

  private sealed class StubAssessmentConfigurationService : IAssessmentConfigurationService
  {
    public Task<AssessmentConfigurationOptions> GetOptionsAsync(CancellationToken cancellationToken)
    {
      return Task.FromResult(new AssessmentConfigurationOptions(Array.Empty<FrameworkConfigurationOption>()));
    }
  }
}
