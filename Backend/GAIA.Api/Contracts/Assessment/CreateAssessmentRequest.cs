using Swashbuckle.AspNetCore.Annotations;

namespace GAIA.Api.Contracts.Assessment;

public record CreateAssessmentRequest(
  [property: SwaggerSchema(Description = "Name of the assessment")]
  string Name,
  [property: SwaggerSchema(Description = "AssessmentDetails start date", Format = "date-time")]
  DateTime StartDate,
  [property: SwaggerSchema(Description = "AssessmentDetails end date", Format = "date-time")]
  DateTime EndDate,
  [property: SwaggerSchema(Description = "Identifier of the organization requesting the assessment")]
  Guid OrganizationId,
  [property: SwaggerSchema(Description = "Organization requesting the assessment")]
  string Organization,
  [property: SwaggerSchema(Description = "Primary language selected for the assessment")]
  string Language
);
