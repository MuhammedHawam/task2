using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GAIA.Api.Contracts;
using GAIA.Core.Configuration.Interfaces;
using GAIA.Core.Commands.Assessment;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GAIA.Api.Controllers;

[ApiController]
[Route("assessments")]
public class AssessmentsController : ControllerBase
{
  private readonly ISender _sender;
  private readonly IAssessmentConfigurationService _configurationService;

  public AssessmentsController(ISender sender, IAssessmentConfigurationService configurationService)
  {
    _sender = sender;
    _configurationService = configurationService;
  }

  [HttpPost]
  public async Task<ActionResult<object>> Create([FromBody] CreateAssessmentRequest request, CancellationToken cancellationToken)
  {
    var result = await _sender.Send(new CreateAssessmentCommand(
        request.Title,
        request.Description,
        request.CreatedBy,
        request.FrameworkId,
        request.AssessmentDepthId,
        request.AssessmentScoringId
    ), cancellationToken);

    return CreatedAtAction(nameof(GetById), new { id = result.AssessmentId }, new { id = result.AssessmentId });
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<object>> GetById(Guid id, CancellationToken cancellationToken)
  {
    // Placeholder to satisfy CreatedAtAction route; implemented later
    return Ok(new { id });
  }

  [HttpGet("configuration/options")]
  public async Task<ActionResult<AssessmentConfigurationOptionsResponse>> GetConfigurationOptions(CancellationToken cancellationToken)
  {
    var options = await _configurationService.GetOptionsAsync(cancellationToken);

    var response = new AssessmentConfigurationOptionsResponse(
      options.Frameworks
        .Select(framework => new FrameworkOptionsDto(
          framework.Id,
          framework.Name,
          framework.AssessmentDepths
            .Select(depth => new AssessmentDepthOptionsDto(
              depth.Id,
              depth.Name,
              depth.AssessmentScorings
                .Select(scoring => new AssessmentScoringOptionsDto(scoring.Id, scoring.Name))
                .ToList()
            ))
            .ToList()
        ))
        .ToList()
    );

    return Ok(response);
  }
}
