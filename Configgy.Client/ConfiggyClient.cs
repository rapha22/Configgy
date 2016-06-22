using System;
using Configgy.Common;
using StackExchange.Redis;

namespace Configgy.Client
{
    public class ConfiggyClient : IDisposable
    {
        private ConnectionMultiplexer _connectionMultiplexer;
        private IConfigurationSpaceStorage _storage;

        public ConfiggyClient(string redisConnectionString, string prefix = null)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);            
            var keyBuilder         = new RedisKeyBuilder(prefix);
            var redisSeverMonitor  = new RedisServerMonitor(_connectionMultiplexer, keyBuilder);
            var redisStorage       = new RedisConfigurationSpaceStorage(_connectionMultiplexer, keyBuilder);
            _storage               = new CachedConfigurationSpaceStorage(redisStorage, redisSeverMonitor);
        }

        public ConfigurationSet GetConfigurationSet(string key)
        {
            return new ConfigurationSet(_storage.Get(key));
        }

        public void Dispose()
        {
            _connectionMultiplexer.Dispose();
        }
    }
}
