namespace PIF.EBP.Application.Shared
{
    public abstract class CacheItemBase<TPrimaryKey>
    {
        public TPrimaryKey Id { get; set; }
    }

    public abstract class CacheItemBase : CacheItemBase<string>
    {

    }
    public interface ICacheItem
    {
       
    }
}
