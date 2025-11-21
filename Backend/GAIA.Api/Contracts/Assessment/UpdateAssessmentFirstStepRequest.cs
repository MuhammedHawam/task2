using Swashbuckle.AspNetCore.Annotations;

namespace GAIA.Api.Contracts.Assessment;

public record UpdateAssessmentFirstStepRequest(
  [property: SwaggerSchema(Description = "Name of the assessment")]
  string Name,
  [property: SwaggerSchema(Description = "Assessment start date", Format = "date-time")]
  DateTime StartDate,
  [property: SwaggerSchema(Description = "Assessment end date", Format = "date-time")]
  DateTime EndDate,
  [property: SwaggerSchema(Description = "Organization requesting the assessment")]
  string Organization,
  [property: SwaggerSchema(Description = "Primary language selected for the assessment")]
  string Language
);
