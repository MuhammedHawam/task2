namespace GAIA.Core.Documents.Models;

public sealed record DocumentDto(Guid Id, byte[] Content, string Status, string Category, string Name);

public sealed record DocumentSummaryDto(Guid Id, string Status, string Category, string Name);

public sealed record DocumentCollectionDto(IReadOnlyList<DocumentSummaryDto> Documents);

public sealed record CreateDocumentDto(byte[] Content, string Status, string Category, string Name);

public sealed record UpdateDocumentDto(string Status, string Category, string Name, byte[]? Content);
