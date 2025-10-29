using System;
using GAIA.Domain.Assessment.Entities;
using GAIA.Domain.DomainEvents;
using Xunit;

namespace GAIA.Tests.Domain.Assessment;

public class AssessmentApplyTests
{
  [Fact]
  public void Apply_Sets_All_Fields_From_Event()
  {
    var id = Guid.NewGuid();
    var createdBy = Guid.NewGuid();
    var frameworkId = Guid.NewGuid();

    var @event = new AssessmentCreated
    {
      Id = id,
      Title = "Risk Assessment",
      Description = "Assess risk across systems",
      CreatedAt = DateTime.UtcNow,
      CreatedBy = createdBy,
      FrameworkId = frameworkId
    };

    var assessment = new Assessment();

    assessment.Apply(@event);

    Assert.Equal(id, assessment.Id);
    Assert.Equal(@event.Title, assessment.Title);
    Assert.Equal(@event.Description, assessment.Description);
    Assert.Equal(@event.CreatedAt, assessment.CreatedAt);
    Assert.Equal(@event.CreatedBy, assessment.CreatedBy);
    Assert.Equal(@event.FrameworkId, assessment.FrameworkId);
  }
}
