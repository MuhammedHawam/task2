using GAIA.Api.Contracts;
using GAIA.Api.Contracts.Assessment;
using GAIA.Api.Mappers;
using GAIA.Core.Assessment.Commands.Assessment;
using GAIA.Core.Assessment.Queries;
using GAIA.Core.Interfaces;
using GAIA.Core.InsightContent.Commands;
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

  [HttpGet("creation/organizations")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "getAssessmentOrganizations",
    Summary = "Get organizations for assessment creation",
    Description = "Returns the list of known organizations referenced by existing assessments."
  )]
  [SwaggerResponse(StatusCodes.Status200OK, "Organizations retrieved successfully.", typeof(OrganizationListResponse))]
  public async Task<ActionResult<OrganizationListResponse>> GetAssessmentOrganizations(
    CancellationToken cancellationToken)
  {
    var organizations = await _sender.Send(new GetAssessmentOrganizationsQuery(), cancellationToken);
    return Ok(organizations.ToResponse());
  }

  [HttpGet("creation/languages")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "getAssessmentLanguages",
    Summary = "Get languages for assessment creation",
    Description = "Returns the languages already used by stored assessments."
  )]
  [SwaggerResponse(StatusCodes.Status200OK, "Languages retrieved successfully.", typeof(LanguageListResponse))]
  public async Task<ActionResult<LanguageListResponse>> GetAssessmentLanguages(
    CancellationToken cancellationToken)
  {
    var languages = await _sender.Send(new GetAssessmentLanguagesQuery(), cancellationToken);
    return Ok(languages.ToResponse());
  }

  [HttpGet("creation/users")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "getAssessmentAccessUsers",
    Summary = "Get users for managing assessment access",
    Description =
      "Returns the list of users that can be granted access to an assessment. Currently returns an empty list until the user source is integrated."
  )]
  [SwaggerResponse(StatusCodes.Status200OK, "Users retrieved successfully.", typeof(UserAccessListResponse))]
  public async Task<ActionResult<UserAccessListResponse>> GetAssessmentAccessUsers(
    CancellationToken cancellationToken)
  {
    var users = await _sender.Send(new GetAssessmentAccessUsersQuery(), cancellationToken);
    return Ok(users.ToResponse());
  }

  [HttpPost("{assessmentId:guid}/users")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "assignUsersToAssessment",
    Summary = "Assign users to an assessment",
    Description = "Adds the provided users to the specified assessment."
  )]
  [SwaggerResponse(StatusCodes.Status200OK, "Users assigned successfully.", typeof(AssessmentUsersResponse))]
  [SwaggerResponse(StatusCodes.Status404NotFound, "Assessment not found.")]
  public async Task<ActionResult<AssessmentUsersResponse>> AssignUsersToAssessment(
    Guid assessmentId,
    [FromBody] AssessmentUsersRequest request,
    CancellationToken cancellationToken)
  {
    if (request.Users is null || request.Users.Count == 0)
    {
      return BadRequest("At least one user must be provided.");
    }

    var command = new AssignUsersToAssessmentCommand(
      assessmentId,
      request.Users
        .Select(user => new AssessmentUserInput(user.Id, user.Username, user.Email, user.Avatar, user.Role))
        .ToList()
    );

    var result = await _sender.Send(command, cancellationToken);

    if (result is null)
    {
      return NotFound();
    }

    return Ok(result.ToResponse());
  }

  [HttpGet("{assessmentId:guid}/users")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "getAssessmentUsers",
    Summary = "Get users assigned to an assessment",
    Description = "Retrieves the users that currently have access to the specified assessment."
  )]
  [SwaggerResponse(StatusCodes.Status200OK, "Assessment users retrieved.", typeof(AssessmentUsersResponse))]
  [SwaggerResponse(StatusCodes.Status404NotFound, "Assessment not found.")]
  public async Task<ActionResult<AssessmentUsersResponse>> GetAssessmentUsers(
    Guid assessmentId,
    CancellationToken cancellationToken)
  {
    var result = await _sender.Send(new GetAssessmentUsersQuery(assessmentId), cancellationToken);

    if (result is null)
    {
      return NotFound();
    }

    return Ok(result.ToResponse());
  }

  [HttpPut("{assessmentId:guid}/users")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "updateAssessmentUsers",
    Summary = "Update users assigned to an assessment",
    Description = "Replaces the assessment's user list with the provided collection."
  )]
  [SwaggerResponse(StatusCodes.Status200OK, "Assessment users updated.", typeof(AssessmentUsersResponse))]
  [SwaggerResponse(StatusCodes.Status404NotFound, "Assessment not found.")]
  public async Task<ActionResult<AssessmentUsersResponse>> UpdateAssessmentUsers(
    Guid assessmentId,
    [FromBody] AssessmentUsersRequest request,
    CancellationToken cancellationToken)
  {
    var users = request.Users ?? Array.Empty<AssessmentUserRequest>();

    var command = new UpdateAssessmentUsersCommand(
      assessmentId,
      users
        .Select(user => new AssessmentUserInput(user.Id, user.Username, user.Email, user.Avatar, user.Role))
        .ToList()
    );

    var result = await _sender.Send(command, cancellationToken);

    if (result is null)
    {
      return NotFound();
    }

    return Ok(result.ToResponse());
  }

  [HttpDelete("{assessmentId:guid}/users")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "removeAssessmentUsers",
    Summary = "Remove users from an assessment",
    Description = "Removes the specified users from the assessment and returns the updated assignment."
  )]
  [SwaggerResponse(StatusCodes.Status200OK, "Assessment users removed.", typeof(AssessmentUsersResponse))]
  [SwaggerResponse(StatusCodes.Status404NotFound, "Assessment not found or no assignments exist.")]
  public async Task<ActionResult<AssessmentUsersResponse>> RemoveAssessmentUsers(
    Guid assessmentId,
    [FromBody] AssessmentUsersDeleteRequest request,
    CancellationToken cancellationToken)
  {
    if (request.UserIds is null || request.UserIds.Count == 0)
    {
      return BadRequest("At least one userId must be provided.");
    }

    var command = new RemoveAssessmentUsersCommand(assessmentId, request.UserIds);

    var result = await _sender.Send(command, cancellationToken);

    if (result is null)
    {
      return NotFound();
    }

    return Ok(result.ToResponse());
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
      request.OrganizationId,
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
      request.OrganizationId,
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
