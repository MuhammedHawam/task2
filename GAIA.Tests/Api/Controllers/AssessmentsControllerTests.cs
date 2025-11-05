using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GAIA.Api.Contracts;
using GAIA.Api.Controllers;
using GAIA.Core.Configuration.Interfaces;
using GAIA.Core.Configuration.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GAIA.Tests.Api.Controllers;

public class AssessmentsControllerTests
{
  private sealed class StubSender : ISender
  {
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
      => throw new NotImplementedException();

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
      => throw new NotImplementedException();
  }

  private sealed class FakeAssessmentConfigurationService : IAssessmentConfigurationService
  {
    private readonly AssessmentConfigurationOptions _options;

    public FakeAssessmentConfigurationService(AssessmentConfigurationOptions options)
    {
      _options = options;
    }

    public Task<AssessmentConfigurationOptions> GetOptionsAsync(CancellationToken cancellationToken)
      => Task.FromResult(_options);
  }

  [Fact]
  public async Task GetConfigurationOptions_ReturnsMappedResponse()
  {
    // Arrange
    var frameworkId = Guid.NewGuid();
    var depthId = Guid.NewGuid();
    var scoringId = Guid.NewGuid();

    var options = new AssessmentConfigurationOptions(new List<FrameworkConfigurationOption>
    {
      new(
        frameworkId,
        "Framework Alpha",
        new List<AssessmentDepthOption>
        {
          new(
            depthId,
            "Depth Level 1",
            new List<AssessmentScoringOption>
            {
              new(scoringId, "Score Bronze")
            }
          )
        }
      )
    });

    var controller = new AssessmentsController(new StubSender(), new FakeAssessmentConfigurationService(options));

    // Act
    var result = await controller.GetConfigurationOptions(CancellationToken.None);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var response = Assert.IsType<AssessmentConfigurationOptionsResponse>(okResult.Value);

    Assert.Single(response.Frameworks);
    var framework = response.Frameworks[0];
    Assert.Equal(frameworkId, framework.Id);
    Assert.Equal("Framework Alpha", framework.Name);

    Assert.Single(framework.AssessmentDepths);
    var depth = framework.AssessmentDepths[0];
    Assert.Equal(depthId, depth.Id);
    Assert.Equal("Depth Level 1", depth.Name);

    Assert.Single(depth.AssessmentScorings);
    var scoring = depth.AssessmentScorings[0];
    Assert.Equal(scoringId, scoring.Id);
    Assert.Equal("Score Bronze", scoring.Name);
  }
}
