using System;
using Configgy.Common;
using StackExchange.Redis;

namespace Configgy.Client
{
    public class ConfiggyClient : IDisposable
    {
        private ConnectionMultiplexer _connectionMultiplexer;
        private IConfigurationSpaceStorage _storage;
        private IConfigurationSetParser _parser;

        public ConfiggyClient(string redisConnectionString, string prefix = null)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);            
            var keyBuilder         = new RedisKeyBuilder(prefix);
            var redisServerMonitor = new RedisServerMonitor(_connectionMultiplexer, keyBuilder);
            var redisStorage       = new RedisConfigurationSpaceStorage(_connectionMultiplexer, keyBuilder);
            var jsonParser         = new JsonConfigurationSetParser();
            
            _storage = new CachedConfigurationSpaceStorage(redisStorage, redisServerMonitor);
            _parser = new CachedConfigurationSetParser(jsonParser, redisServerMonitor);

            redisServerMonitor.Start();
        }

        public ConfigurationSet GetConfigurationSet(string key)
        {
            return new ConfigurationSet(_storage.Get(key), _parser);
        }

        public void Dispose()
        {
            _connectionMultiplexer.Dispose();
        }
    }
}
