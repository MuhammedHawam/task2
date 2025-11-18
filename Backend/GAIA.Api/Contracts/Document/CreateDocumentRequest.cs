namespace GAIA.Api.Contracts.Document;

public record CreateDocumentRequest(
  string Name,
  string Status,
  string Category,
  string? Content = null
);
