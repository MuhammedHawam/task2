using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GAIA.Api.Contracts;
using GAIA.Core.Assessment.Queries;
using GAIA.Core.Configuration.Interfaces;
using GAIA.Core.Commands.Assessment;
using GAIA.Domain.Assessment.Entities;
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

  [HttpGet]
  public async Task<ActionResult<IReadOnlyList<AssessmentResponse>>> GetAll(CancellationToken cancellationToken)
  {
    var assessments = await _sender.Send(new GetAssessmentsQuery(), cancellationToken);
    var response = assessments.Select(MapToResponse).ToList();

    return Ok(response);
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<AssessmentResponse>> GetById(Guid id, CancellationToken cancellationToken)
  {
    var assessment = await _sender.Send(new GetAssessmentByIdQuery(id), cancellationToken);

    if (assessment is null)
    {
      return NotFound();
    }

    return Ok(MapToResponse(assessment));
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

  private static AssessmentResponse MapToResponse(Assessment assessment) => new(
    assessment.Id,
    assessment.Title,
    assessment.Description,
    assessment.CreatedAt,
    assessment.CreatedBy,
    assessment.FrameworkId,
    assessment.AssessmentDepthId,
    assessment.AssessmentScoringId
  );
}
