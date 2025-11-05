using Swashbuckle.AspNetCore.Annotations;

namespace GAIA.Api.Contracts
{
  /// <summary>
  /// Request body used to create a new assessment.
  /// </summary>
  /// <remarks>
  /// Consumed by POST /assessments.
  /// </remarks>
  /// <param name="Title">Human-readable title of the assessment.</param>
  /// <param name="Description">Detailed description of the assessment.</param>
  /// <param name="CreatedBy">Identifier of the user creating the assessment (UUID).</param>
  /// <param name="FrameworkId">Identifier of the framework this assessment uses (UUID).</param>
  /// <param name="AssessmentDepthId">Identifier of the chosen assessment depth (UUID).</param>
  /// <param name="AssessmentScoringId">Identifier of the chosen assessment scoring model (UUID).</param>
  public record CreateAssessmentRequest(
    [property: SwaggerSchema(Description = "Human-readable title of the assessment")]
    string Title,
    [property: SwaggerSchema(Description = "Detailed description of the assessment")]
    string Description,
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
