using StackExchange.Redis;

namespace PIF.EBP.Core.Caching
{
    public interface ITypedCache
    {
        ConnectionMultiplexer Connection { get; }
    }
}
