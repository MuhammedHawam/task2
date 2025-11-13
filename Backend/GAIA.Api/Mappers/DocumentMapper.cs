using GAIA.Api.Contracts.Documents;
using GAIA.Core.Documents.Models;

namespace GAIA.Api.Mappers;

public static class DocumentMapper
{
  public static DocumentResponse ToResponse(this DocumentDto document)
  {
    return new DocumentResponse(
      document.Id,
      Convert.ToBase64String(document.Content),
      document.Status,
      document.Category,
      document.Name);
  }

  public static DocumentSummaryResponse ToResponse(this DocumentSummaryDto document)
  {
    return new DocumentSummaryResponse(
      document.Id,
      document.Status,
      document.Category,
      document.Name);
  }
}
