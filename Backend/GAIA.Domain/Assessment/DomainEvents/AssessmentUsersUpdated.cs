using System;
using System.Collections.Generic;

namespace GAIA.Domain.Assessment.DomainEvents;

public class AssessmentUsersUpdated
{
  public Guid AssessmentId { get; set; }
  public IReadOnlyList<AssessmentUserSnapshot> Users { get; set; } = Array.Empty<AssessmentUserSnapshot>();
  public DateTime UpdatedAt { get; set; }
}
