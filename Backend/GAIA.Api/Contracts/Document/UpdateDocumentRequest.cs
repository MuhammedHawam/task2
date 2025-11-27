namespace GAIA.Api.Contracts.Document;

public record UpdateDocumentRequest(
  string? Name,
  string? Status,
  string? Category,
  string? Content = null
);

