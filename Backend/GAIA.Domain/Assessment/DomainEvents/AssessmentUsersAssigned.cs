using System;
using System.Collections.Generic;

namespace GAIA.Domain.Assessment.DomainEvents;

public class AssessmentUsersAssigned
{
  public Guid AssessmentId { get; set; }
  public IReadOnlyList<AssessmentUserSnapshot> Users { get; set; } = Array.Empty<AssessmentUserSnapshot>();
  public DateTime AssignedAt { get; set; }
}
