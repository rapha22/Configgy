using Configgy.Common;
using StackExchange.Redis;

namespace Configgy.Client
{
    public class RedisConfigurationSpaceStorage : IConfigurationSpaceStorage
    {
        private ConnectionMultiplexer _connectionMultiplexer;
        private RedisKeyBuilder _keyBuilder;

        public RedisConfigurationSpaceStorage(ConnectionMultiplexer connectionMultiplexer, RedisKeyBuilder keyBuilder)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _keyBuilder = keyBuilder;
        }

        public string Get(string key)
        {
            return _connectionMultiplexer.GetDatabase().StringGet(_keyBuilder.BuildKey(key));
        }
    }
}
