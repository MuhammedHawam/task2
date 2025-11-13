namespace GAIA.Api.Contracts.Documents;

public sealed record CreateDocumentRequest(
  string Content,
  string Status,
  string Category,
  string Name);
