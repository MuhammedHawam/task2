namespace GAIA.Api.Contracts.Documents
{
  public sealed record DocumentResponse(Guid Id, string Status, string Category, string Name, string ContentBase64);
}
