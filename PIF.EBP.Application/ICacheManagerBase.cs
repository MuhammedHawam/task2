using PIF.EBP.Application.Shared;
using System.Threading.Tasks;

namespace PIF.EBP.Application
{
    public interface ICacheManagerBase<TCacheItem, TPrimaryKey> where TCacheItem : CacheItemBase<TPrimaryKey>
    {
        Task ClearCacheAsync();

    }

    public interface ICacheManagerBase<TCacheItem> : ICacheManagerBase<TCacheItem, string>
        where TCacheItem : CacheItemBase<string>
    {

    }
}
