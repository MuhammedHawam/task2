namespace GAIA.Api.Contracts.Document;

public record DocumentResponse(
  Guid Id,
  string Name,
  string Status,
  string Category,
  DateTime CreatedAt,
  DateTime? UpdatedAt
);

public record DocumentsListResponse(
  List<DocumentResponse> Documents
);

public record DocumentStatusCategoryResponse(
  Guid Id,
  string Status,
  string Category,
  string Name
);

public record DocumentsStatusCategoryListResponse(
  List<DocumentStatusCategoryResponse> Documents
);
