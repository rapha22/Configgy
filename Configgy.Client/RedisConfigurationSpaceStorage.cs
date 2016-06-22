using Configgy.Common;
using Newtonsoft.Json;
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

        public object Get(string key)
        {
            var redis = _connectionMultiplexer.GetDatabase();
            var json = redis.StringGet(_keyBuilder.BuildKey(key));
            return JsonConvert.DeserializeObject(json);
        }
    }
}
