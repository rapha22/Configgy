using System;
using Configgy.Common;
using StackExchange.Redis;

namespace Configgy.Client
{
    internal class RedisServerMonitor : IServerMonitor
    {
        private ConnectionMultiplexer _connectionMultiplexer;
        private RedisKeyBuilder _keyBuilder;
        private RedisChannel _channel;

        public event Action ConfigurationSpaceRebuilt;

        public RedisServerMonitor(ConnectionMultiplexer connectionMultiplexer, RedisKeyBuilder keyBuilder)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _keyBuilder = keyBuilder;
            _channel = new RedisChannel(_keyBuilder.BuildKey("*"), RedisChannel.PatternMode.Pattern);
        }

        public void Start()
        {
            var subscriber = _connectionMultiplexer.GetSubscriber();

            subscriber.Subscribe(_channel, (channel, message) =>
            {
                if (message == RedisMessages.ConfigurationSpaceRebuilt)
                {
                    if (ConfigurationSpaceRebuilt != null)
                        ConfigurationSpaceRebuilt();
                }
            });
        }
    }
}
