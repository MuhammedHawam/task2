using GAIA.Api.Contracts.Assessment;
using GAIA.Api.Mappers;
using GAIA.Core.Assessment.Commands.Assessment;
using GAIA.Core.Assessment.Interfaces;
using GAIA.Core.Assessment.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

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

  [HttpGet]
  public async Task<ActionResult<IReadOnlyList<AssessmentResponse>>> GetAll(CancellationToken cancellationToken)
  {
    var assessments = await _sender.Send(new GetAssessmentsQuery(), cancellationToken);
    var response = assessments.Select(assessment => assessment.ToResponse()).ToList();

    return Ok(response);
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
    var assessment = await _sender.Send(new GetAssessmentByIdQuery(id), cancellationToken);

    if (assessment is null)
    {
      return NotFound();
    }

    return Ok(assessment.ToResponse());
  }

  /// <summary>
  /// Retrieves the available assessment configuration options grouped by framework.
  /// </summary>
  /// <remarks>
  /// Provides the list of frameworks along with their supported assessment depths and scoring profiles.
  ///
  /// Sample request:
  /// <code>GET /assessments/configuration/options</code>
  ///
  /// Sample response:
  /// {
  ///   "frameworks": [
  ///     {
  ///       "id": "ad2c63f9-b856-4e36-91f9-5da7b42e1f33",
  ///       "name": "Cybersecurity Maturity",
  ///       "assessmentDepths": [
  ///         { "id": "982b6a76-a312-46c5-9a9d-34175e5b56dd", "name": "Initial", "depth": 1 }
  ///       ],
  ///       "assessmentScorings": [
  ///         { "id": "d758ff3c-fb19-4a88-bb29-9a48b204361e", "name": "Bronze" }
  ///       ]
  ///     }
  ///   ]
  /// }
  ///
  /// Possible status codes:
  /// * 200 - Configuration options retrieved successfully.
  /// * 500 - An unexpected error occurred while retrieving the options.
  /// </remarks>
  [HttpGet("configuration/options")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
  Summary = "Get assessment configuration options",
  Description = "Returns the available frameworks with their assessment depths and scoring profiles so that clients can populate configuration UI."
)]
  [SwaggerResponse(StatusCodes.Status200OK, "Configuration options retrieved successfully.", typeof(AssessmentConfigurationOptionsResponse))]
  [SwaggerResponse(StatusCodes.Status500InternalServerError, "Unexpected error while retrieving configuration options.")]
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
                depth.Depth
              ))
              .ToList(),
            framework.AssessmentScorings
              .Select(scoring => new AssessmentScoringOptionsDto(scoring.Id, scoring.Name))
              .ToList()
        ))
        .ToList()
    );

    return Ok(response);
  }
}
