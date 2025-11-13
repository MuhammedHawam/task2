namespace GAIA.Api.Contracts.Documents;

public sealed record DocumentsResponse(IReadOnlyList<DocumentSummaryResponse> Documents);
