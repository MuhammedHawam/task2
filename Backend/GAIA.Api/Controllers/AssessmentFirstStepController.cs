using System.Net.Mime;
using GAIA.Api.Contracts.Assessment;
using GAIA.Api.Mappers;
using GAIA.Core.Assessment.Commands.FirstStep;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GAIA.Api.Controllers;

[ApiController]
[Route("api/Assessment")]
public class AssessmentFirstStepController : ControllerBase
{
  private readonly ISender _sender;

  public AssessmentFirstStepController(ISender sender)
  {
    _sender = sender;
  }

  [HttpPost("CreateAssessmentFirstStep")]
  [Produces(MediaTypeNames.Application.Json)]
  [SwaggerOperation(
    OperationId = "createAssessmentFirstStep",
    Summary = "Create the first step of an assessment",
    Description = "Creates the basic assessment details (name, dates, organization, language) and returns the persisted data."
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

  [HttpPut("UpdateAssessmentFirstStep")]
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
}
