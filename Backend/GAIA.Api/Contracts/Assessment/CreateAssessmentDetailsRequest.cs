using Swashbuckle.AspNetCore.Annotations;

namespace GAIA.Api.Contracts.Assessment
{
  /// <summary>
  /// Request body used to create a new assessment details.
  /// </summary>
  /// <remarks>
  /// Consumed by POST /assessmentsdetails.
  /// </remarks>
  /// <param name="Title">Title of the assessment.</param>
  /// <param name="Description">Detailed description of the assessment.</param>
  /// <param name="AssessmentId">Identifier of the assessment that owns these details (UUID).</param>
  /// <param name="CreatedBy">Identifier of the user creating the assessment (UUID).</param>
  /// <param name="FrameworkId">Identifier of the framework this assessment uses (UUID).</param>
  public record CreateAssessmentDetailsRequest(
    [property: SwaggerSchema(Description = "Title of the assessment")]
    string Title,
    [property: SwaggerSchema(Description = "Detailed description of the assessment")]
    string Description,
    [property: SwaggerSchema(Description = "Identifier of the assessment that owns these details", Format = "uuid")]
    Guid AssessmentId,
    [property: SwaggerSchema(Description = "Identifier of the user creating the assessment", Format = "uuid")]
    Guid CreatedBy,
    [property: SwaggerSchema(Description = "Identifier of the framework this assessment uses", Format = "uuid")]
    Guid FrameworkId,
    [property: SwaggerSchema(Description = "Identifier of the chosen assessment depth", Format = "uuid")]
    Guid AssessmentDepthId,
    [property: SwaggerSchema(Description = "Identifier of the chosen assessment scoring model", Format = "uuid")]
    Guid AssessmentScoringId
 );
}
