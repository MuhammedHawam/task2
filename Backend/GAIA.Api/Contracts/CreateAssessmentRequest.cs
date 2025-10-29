namespace GAIA.Api.Contracts
{
  public record CreateAssessmentRequest(
     string Title,
     string Description,
     Guid CreatedBy,
     Guid FrameworkId
 );
}
