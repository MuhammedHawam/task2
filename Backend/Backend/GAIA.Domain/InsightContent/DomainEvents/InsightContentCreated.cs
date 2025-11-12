namespace GAIA.Domain.InsightContent.DomainEvents
{
  public class InsightContentCreated
  {
    public Guid Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid AssessmentId { get; set; }
  }
}
