using GAIA.Core.Documents.Models;
using GAIA.Domain.Documents.Entities;

namespace GAIA.Core.Documents.Mappers;

public static class DocumentMappingExtensions
{
  public static DocumentDto ToDto(this Document document)
  {
    return new DocumentDto(document.Id, document.Content, document.Status, document.Category, document.Name);
  }

  public static DocumentSummaryDto ToSummaryDto(this Document document)
  {
    return new DocumentSummaryDto(document.Id, document.Status, document.Category, document.Name);
  }
}
