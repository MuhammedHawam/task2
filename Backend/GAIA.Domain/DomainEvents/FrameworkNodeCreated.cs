using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAIA.Domain.DomainEvents
{
  public class FrameworkNodeCreated
  {
    public Guid Id { get; set; }
    public int Depth { get; set; }
    public string Content { get; set; }
  }
}
