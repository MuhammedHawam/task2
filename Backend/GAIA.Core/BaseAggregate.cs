namespace GAIA.Core;

public interface IBaseAggregate
{
  Guid Id { get; }
  DateTime CreatedAt { get; }
  DateTime UpdatedAt { get; }
}

public abstract class IdentityObject
{
  public Guid Id { get; set; }
}

public abstract class BaseAggregate : IdentityObject, IBaseAggregate
{
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  protected void _Initialize()
  {
    Id = Guid.NewGuid();
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = CreatedAt;
  }
}
