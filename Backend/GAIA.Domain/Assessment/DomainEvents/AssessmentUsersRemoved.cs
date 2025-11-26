using System;
using System.Collections.Generic;

namespace GAIA.Domain.Assessment.DomainEvents;

public class AssessmentUsersRemoved
{
  public Guid AssessmentId { get; set; }
  public IReadOnlyList<Guid> UserIds { get; set; } = Array.Empty<Guid>();
  public DateTime RemovedAt { get; set; }
}
