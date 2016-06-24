using System.Collections.Generic;
using Configgy.Common;
using StackExchange.Redis;

namespace Configgy.Server
{
    public class RedisStorageResourcesFactory
    {
        private ConnectionMultiplexer _connectionMultiplexer;
        private RedisKeyBuilder _keyBuilder;
        private RedisStorage _storage;
        private RedisStorageMonitor _monitor;
        private RedisEventBroadcaster _broadcaster;
        private ILogger _logger;

        public RedisStorageResourcesFactory(string redisConnectionString, string prefix, ILogger logger)
        {
            var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
            redisOptions.AllowAdmin = true;

            _connectionMultiplexer = ConnectionMultiplexer.Connect(redisOptions);
            _logger = logger;
            
            _keyBuilder  = new RedisKeyBuilder(prefix);
            _monitor     = new RedisStorageMonitor(_connectionMultiplexer, _keyBuilder, _logger);
            _storage     = new RedisStorage(_connectionMultiplexer, _keyBuilder, _monitor);
            _broadcaster = new RedisEventBroadcaster(_connectionMultiplexer, _keyBuilder);
        }

        public IStorage GetStorage()
        {
            return _storage;
        }

        public IEventBroadcaster GetEventBroadcaster()
        {
            return _broadcaster;
        }

        public IEnumerable<IMonitor> GetMonitors()
        {
            return new[] { _monitor };
        }
    }
}
