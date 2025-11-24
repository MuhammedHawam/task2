using Swashbuckle.AspNetCore.Annotations;

namespace GAIA.Api.Contracts.Assessment;

/// <summary>
/// Represents the payload used to update the content of an Insight.
/// </summary>
/// <param name="Content">The updated Insight content.</param>
public record UpdateInsightContentRequest(
  [property: SwaggerSchema(Description = "The updated Insight content provided by the user.")]
  string Content
);
