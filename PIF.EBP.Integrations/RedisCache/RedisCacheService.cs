using PIF.EBP.Core.Caching;
using StackExchange.Redis;
using System.Configuration;

namespace PIF.EBP.Integrations.RedisCache
{
    public class RedisCacheService : ITypedCache
    {
        private ConnectionMultiplexer _connection;

        public RedisCacheService()
        {
            if (_connection == null || !_connection.IsConnected)
            {
                if (_connection != null)
                {
                    _connection.Dispose();
                }
                _connection = Create();
            }
        }

        public ConnectionMultiplexer Connection { 
            get 
            {
                if (_connection == null || !_connection.IsConnected)
                {
                    if(_connection != null)
                    {
                        _connection.Dispose();
                    }
                    _connection = Create();
                }
                return _connection;
            }
        } 

        private ConnectionMultiplexer Create()
        {
            var configuration = ConfigurationManager.AppSettings["RedisURL"];
            var configurationOptions = ConfigurationOptions.Parse(configuration);
            configurationOptions.AbortOnConnectFail = false; // Prevents failure if Redis is unavailable on startup
            configurationOptions.ReconnectRetryPolicy = new ExponentialRetry(5000); // Retry every 5 seconds
            configurationOptions.ConnectTimeout = 5000; // 5 seconds
            configurationOptions.KeepAlive = 180; // 3 minutes
            return ConnectionMultiplexer.Connect(configurationOptions);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
