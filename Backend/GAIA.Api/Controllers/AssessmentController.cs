using GAIA.Api.Contracts;
using GAIA.Api.Contracts.Assessment;
using GAIA.Api.Mappers;
using GAIA.Core.Assessment.Commands.Assessment;
using GAIA.Core.Assessment.Queries;
using GAIA.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using FrameworkOptionsDto = GAIA.Api.Contracts.FrameworkOptionsDto;

namespace GAIA.Api.Controllers;

[ApiController]
[Route("assessments")]
public class AssessmentsController : ControllerBase
{
  private readonly ISender _sender;
  private readonly IFrameworkService _frameworkService;

  public AssessmentsController(ISender sender, IFrameworkService frameworkService)
  {
    _sender = sender;
    _frameworkService = frameworkService;
  }

  [HttpGet("GetAllAssessmentDetails")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "getAssessmentsDetails",
    Summary = "Get assessments details",
    Description = "Returns the available assessments."
  )]
  public async Task<ActionResult<IReadOnlyList<AssessmentDetailsResponse>>> GetAll(CancellationToken cancellationToken)
  {
    var assessments = await _sender.Send(new GetAssessmentsDetailsQuery(), cancellationToken);
    var response = assessments.Select(assessment => assessment.ToResponse()).ToList();

    return Ok(response);
  }

  [HttpPost("CreateAssessmentDetails")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "createAssessmentDetails",
    Summary = "Create new assessment details",
    Description = "Returns the created assessment ID."
  )]
  public async Task<ActionResult<AssessmentDetailsResponse>> Create([FromBody] CreateAssessmentDetailsRequest request,
    CancellationToken cancellationToken)
  {
    var result = await _sender.Send(new CreateAssessmentDetailsCommand(
      request.Title,
      request.Description,
      request.CreatedBy,
      request.FrameworkId,
      request.AssessmentDepthId,
      request.AssessmentScoringId
    ), cancellationToken);

    return CreatedAtAction(nameof(GetById), new { id = result.AssessmentId }, new { id = result.AssessmentId });
  }

  [HttpGet("GetAssessmentDetails/{id:guid}")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "getAssessmentDetailsById",
    Summary = "Get assessment details by ID",
    Description = "Returns the assessment details for the specified ID."
  )]
  public async Task<ActionResult<object>> GetById(Guid id, CancellationToken cancellationToken)
  {
    var assessment = await _sender.Send(new GetAssessmentDetailsByIdQuery(id), cancellationToken);

    if (assessment is null)
    {
      return NotFound();
    }

    return Ok(assessment.ToResponse());
  }

  // TODO: Move to FrameworksController?
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
        OperationId = "getAssessmentConfigurationOptions",
    Summary = "Get assessment configuration options",
    Description =
      "Returns the available frameworks with their assessment depths and scoring profiles so that clients can populate configuration UI."
  )]
  [SwaggerResponse(StatusCodes.Status200OK, "Configuration options retrieved successfully.",
    typeof(FrameworkConfigurationOptionsResponse))]
  [SwaggerResponse(StatusCodes.Status500InternalServerError,
    "Unexpected error while retrieving configuration options.")]
  public async Task<ActionResult<FrameworkConfigurationOptionsResponse>> GetConfigurationOptions(
    CancellationToken cancellationToken)
  {
    var frameworks = await _frameworkService.ListFrameworksWithOptions();

    var response = new FrameworkConfigurationOptionsResponse(
      frameworks
        .Select(framework => new FrameworkOptionsDto(
          framework.Id,
          framework.Name,
          framework.AssessmentDepths
            .Select(depth => new FrameworkDepthOptionsDto(
              depth.Id,
              depth.Name,
              depth.Depth
            ))
            .ToList(),
          framework.AssessmentScorings
            .Select(scoring => new FrameworkScoringOptionsDto(scoring.Id, scoring.Name))
            .ToList()
        ))
        .ToList()
    );

    return Ok(response);
  }

  [HttpPost("CreateAssessment")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "createAssessment",
    Summary = "Create the first step of an assessment",
    Description =
      "Creates the basic assessment details (name, dates, organization, language) and returns the persisted data."
  )]
  [SwaggerResponse(StatusCodes.Status200OK, "AssessmentDetails first step created.", typeof(AssessmentResponse))]
  public async Task<ActionResult<AssessmentResponse>> CreateAssessment(
    [FromBody] CreateAssessmentRequest request,
    CancellationToken cancellationToken)
  {
    var result = await _sender.Send(new CreateAssessmentCommand(
      request.Name,
      request.StartDate,
      request.EndDate,
      request.Organization,
      request.Language
    ), cancellationToken);

    return Ok(result.ToResponse());
  }

  [HttpPut("UpdateAssessment")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "updateAssessment",
    Summary = "Update the first step of an assessment",
    Description = "Updates the previously saved first-step details for the specified assessment."
  )]
  [SwaggerResponse(StatusCodes.Status200OK, "AssessmentDetails first step updated.", typeof(AssessmentResponse))]
  [SwaggerResponse(StatusCodes.Status404NotFound, "AssessmentDetails first step not found.")]
  public async Task<ActionResult<AssessmentResponse>> UpdateAssessment(
    [FromQuery] Guid assessmentId,
    [FromBody] UpdateAssessmentRequest request,
    CancellationToken cancellationToken)
  {
    if (assessmentId == Guid.Empty)
    {
      return BadRequest("assessmentId query parameter is required.");
    }

    var result = await _sender.Send(new UpdateAssessmentCommand(
      assessmentId,
      request.Name,
      request.StartDate,
      request.EndDate,
      request.Organization,
      request.Language
    ), cancellationToken);

    if (result is null)
    {
      return NotFound();
    }

    return Ok(result.ToResponse());
  }

  [HttpGet("GetAssessments")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "getAssessments",
    Summary = "Get assessments",
    Description = "Returns the first-step metadata for all assessments."
  )]
  [SwaggerResponse(StatusCodes.Status200OK, "Assessment data retrieved.",
    typeof(IReadOnlyList<AssessmentResponse>))]
  public async Task<ActionResult<IReadOnlyList<AssessmentResponse>>> GetAssessments(
    CancellationToken cancellationToken)
  {
    var assessments = await _sender.Send(new GetAssessmentsQuery(), cancellationToken);
    var response = assessments.Select(step => step.ToResponse()).ToList();
    return Ok(response);
  }

  [HttpGet("{assessmentId:guid}")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "getAssessmentById",
    Summary = "Get assessment by ID",
    Description = "Returns the first-step details for the specified assessment."
  )]
  [SwaggerResponse(StatusCodes.Status200OK, "Assessment retrieved.", typeof(AssessmentResponse))]
  [SwaggerResponse(StatusCodes.Status404NotFound, "Assessment not found.")]
  public async Task<ActionResult<AssessmentResponse>> GetAssessmentById(Guid assessmentId,
    CancellationToken cancellationToken)
  {
    var assessment = await _sender.Send(new GetAssessmentByIdQuery(assessmentId), cancellationToken);
    if (assessment is null)
    {
      return NotFound();
    }

    return Ok(assessment.ToResponse());
  }

}
