namespace GAIA.Api.Contracts.Documents;

public sealed record UpdateDocumentRequest(
  string Status,
  string Category,
  string Name,
  string? Content);
