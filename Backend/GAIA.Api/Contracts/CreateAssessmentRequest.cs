namespace GAIA.Api.Contracts
{
  /// <summary>
  /// Request payload to create a new assessment.
  /// </summary>
  /// <param name="Title">Human-friendly name of the assessment.</param>
  /// <param name="Description">Detailed description of the assessment’s purpose and scope.</param>
  /// <param name="CreatedBy">Identifier of the user creating the assessment.</param>
  /// <param name="FrameworkId">Identifier of the framework this assessment is based on.</param>
  public record CreateAssessmentRequest(
     string Title,
     string Description,
     Guid CreatedBy,
     Guid FrameworkId
 );
}
