namespace GAIA.Api.Contracts.Documents;

public sealed record DocumentResponse(
  Guid Id,
  string Content,
  string Status,
  string Category,
  string Name);
