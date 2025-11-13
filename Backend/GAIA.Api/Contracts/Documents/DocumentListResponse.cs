using System.Text.Json.Serialization;

namespace GAIA.Api.Contracts.Documents
{
  public sealed record DocumentListResponse([property: JsonPropertyName("Documents")] IReadOnlyList<DocumentSummaryResponse> Documents);
}
