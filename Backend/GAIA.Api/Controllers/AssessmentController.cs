using System.Net.Mime;
using GAIA.Api.Contracts;
using GAIA.Api.Contracts.Assessment;
using GAIA.Api.Mappers;
using GAIA.Core.Assessment.Commands.Assessment;
using GAIA.Core.Assessment.Commands.FirstStep;
using GAIA.Core.Assessment.Queries;
using GAIA.Core.Interfaces;
using GAIA.Core.InsightContent.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
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

  [HttpGet]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "getAssessments",
    Summary = "Get assessments",
    Description = "Returns the available assessments."
  )]
  public async Task<ActionResult<IReadOnlyList<AssessmentResponse>>> GetAll(CancellationToken cancellationToken)
  {
    var assessments = await _sender.Send(new GetAssessmentsQuery(), cancellationToken);
    var response = assessments.Select(assessment => assessment.ToResponse()).ToList();

    return Ok(response);
  }

  [HttpPost]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "createAssessment",
    Summary = "Create new assessments",
    Description = "Returns the created assessment ID."
  )]
  public async Task<ActionResult<AssessmentResponse>> Create([FromBody] CreateAssessmentRequest request,
    CancellationToken cancellationToken)
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

  [HttpPost("/api/Assessment/CreateAssessmentFirstStep")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "createAssessmentFirstStep",
    Summary = "Create the first step of an assessment",
    Description =
      "Creates the basic assessment details (name, dates, organization, language) and returns the persisted data."
  )]
  [SwaggerResponse(StatusCodes.Status200OK, "Assessment first step created.", typeof(AssessmentFirstStepResponse))]
  public async Task<ActionResult<AssessmentFirstStepResponse>> CreateFirstStep(
    [FromBody] CreateAssessmentFirstStepRequest request,
    CancellationToken cancellationToken)
  {
    var result = await _sender.Send(new CreateAssessmentFirstStepCommand(
      request.Name,
      request.StartDate,
      request.EndDate,
      request.Organization,
      request.Language
    ), cancellationToken);

    return Ok(result.ToResponse());
  }

  [HttpPut("/api/Assessment/UpdateAssessmentFirstStep")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "updateAssessmentFirstStep",
    Summary = "Update the first step of an assessment",
    Description = "Updates the previously saved first-step details for the specified assessment."
  )]
  [SwaggerResponse(StatusCodes.Status200OK, "Assessment first step updated.", typeof(AssessmentFirstStepResponse))]
  [SwaggerResponse(StatusCodes.Status404NotFound, "Assessment first step not found.")]
  public async Task<ActionResult<AssessmentFirstStepResponse>> UpdateFirstStep(
    [FromQuery] Guid assessmentId,
    [FromBody] UpdateAssessmentFirstStepRequest request,
    CancellationToken cancellationToken)
  {
    if (assessmentId == Guid.Empty)
    {
      return BadRequest("assessmentId query parameter is required.");
    }

    var result = await _sender.Send(new UpdateAssessmentFirstStepCommand(
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

  [HttpGet("{id:guid}")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "getAssessmentById",
    Summary = "Get assessment by ID",
    Description = "Returns the assessment details for the specified ID."
  )]
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
  /// Updates (or creates) Insight content for an assessment.
  /// </summary>
  /// <remarks>
  /// Prior to the AI integration this behaves like an upsert operation.
  /// </remarks>
  [HttpPut("{assessmentId:guid}/insights/{insightId:guid}/updateContent")]
  [HttpPut("/api/assessments/{assessmentId:guid}/insights/{insightId:guid}/updateContent")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    Summary = "Update Insight content",
    Description = "Allows a user to provide manual content for an Insight and emits a UserUpdatedInsightEvent."
  )]
  [SwaggerResponse(StatusCodes.Status200OK, "Insight content updated.", typeof(UpdateInsightContentResponse))]
  [SwaggerResponse(StatusCodes.Status404NotFound, "Assessment not found.")]
  public async Task<ActionResult<UpdateInsightContentResponse>> UpdateInsightContent(
    Guid assessmentId,
    Guid insightId,
    [FromBody] UpdateInsightContentRequest request,
    CancellationToken cancellationToken)
  {
    var result =
      await _sender.Send(new UpdateInsightContentCommand(assessmentId, insightId, request.Content), cancellationToken);

    if (result is null)
    {
      return NotFound();
    }

    var response = new UpdateInsightContentResponse(result.AssessmentId, result.InsightId, result.Content,
      result.CreatedNew);
    return Ok(response);
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
}
