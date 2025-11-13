namespace GAIA.Api.Contracts.Documents;

public sealed record DocumentSummaryResponse(
  Guid Id,
  string Status,
  string Category,
  string Name);
