using GAIA.Api.Contracts;
using GAIA.Core.Commands.Assessment;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GAIA.Api.Controllers;

[ApiController]
[Route("assessments")]
public class AssessmentsController : ControllerBase
{
  private readonly ISender _sender;

  public AssessmentsController(ISender sender)
  {
    _sender = sender;
  }

  [HttpPost]
  public async Task<ActionResult<object>> Create([FromBody] CreateAssessmentRequest request, CancellationToken cancellationToken)
  {
    var result = await _sender.Send(new CreateAssessmentCommand(
        request.Title,
        request.Description,
        request.CreatedBy,
        request.FrameworkId
    ), cancellationToken);

    return CreatedAtAction(nameof(GetById), new { id = result.AssessmentId }, new { id = result.AssessmentId });
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<object>> GetById(Guid id, CancellationToken cancellationToken)
  {
    // Placeholder to satisfy CreatedAtAction route; implemented later
    return Ok(new { id });
  }
}
