using GAIA.Domain.Assessment.Entities;
using GAIA.Domain.DomainEvents;
using Marten.Events.Aggregation;

namespace GAIA.Infra.Projections
{
  public class AssessmentProjection : SingleStreamProjection<Assessment, Guid>
  {
    public void Apply(AssessmentCreated e, Assessment assessment)
    {
      assessment.Id = e.Id;
      assessment.Title = e.Title;
      assessment.Description = e.Description;
      assessment.CreatedAt = e.CreatedAt;
      assessment.CreatedBy = e.CreatedBy;
      assessment.FrameworkId = e.FrameworkId;
      assessment.AssessmentDepthId = e.AssessmentDepthId;
      assessment.AssessmentScoringId = e.AssessmentScoringId;
    }
  }
}
