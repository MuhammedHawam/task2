using PIF.EBP.Application.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application
{
    public interface ICrmQueriesBase
    {

        Task<IEnumerable<TCacheItem>> GetItemFromDataSource<TCacheItem, TPrimaryKey>(string entityName)
            where TCacheItem : CacheItemBase<TPrimaryKey>;
        Task<List<T>> GetFromDataSource<T>(string entityName) where T : ICacheItem;
    }
}
