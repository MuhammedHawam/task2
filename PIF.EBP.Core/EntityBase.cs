namespace PIF.EBP.Core
{
    public class EntityBase<TPrimaryKey>
    {
        public TPrimaryKey Id { get; set; }
    }

    public class EntityBase : EntityBase<string>
    {

    }
}
