using Configgy.Common;
using StackExchange.Redis;

namespace Configgy.Server
{
    public class RedisEventBroadcaster : IEventBroadcaster
    {
        private ConnectionMultiplexer _connectionMultiplexer;
        private RedisKeyBuilder _keyBuilder;

        public RedisEventBroadcaster(ConnectionMultiplexer connectionMultiplexer, RedisKeyBuilder keyBuilder)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _keyBuilder = keyBuilder;
        }

        public void BroadcastConfigurationSpaceRebuilt()
        {
            var db = _connectionMultiplexer.GetDatabase();

            db.Publish(
                channel: _keyBuilder.BuildKey(RedisChannels.ClientMessages),
                message: RedisMessages.ConfigurationSpaceRebuilt
            );
        }
    }
}
