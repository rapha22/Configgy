using System;
using System.Collections.Generic;
using Configgy.Common;
using StackExchange.Redis;

namespace Configgy.Server
{
    public class RedisStorageChangeMonitor : IDisposable
    {
        private const string KeyspaceEventsPrefix = "__keyspace@0__:";

        private ConnectionMultiplexer _redisConnectionMultiplexer;
        private RedisChannel _keyEventsChannel;
        private bool _isPaused = false;
        private ILogger _logger;

        public ICollection<string> KeysToIgnore { get; private set; }
        public event Action<string, string> KeyChanged;

        public RedisStorageChangeMonitor(ConnectionMultiplexer redisConnectionMultiplexer, RedisKeyBuilder keyBuilder, ILogger logger)
        {
            _redisConnectionMultiplexer = redisConnectionMultiplexer;
            _logger = logger;

            var channelPattern = KeyspaceEventsPrefix + keyBuilder.BuildKey("*");
            _keyEventsChannel = new RedisChannel(channelPattern, RedisChannel.PatternMode.Pattern);

            KeysToIgnore = new List<string>();

            StartListening();
        }


        public void Start()
        {
            _isPaused = false;
        }

        public void Pause()
        {
            _isPaused = true;
        }

        public void Stop()
        {
            _redisConnectionMultiplexer.GetSubscriber().Unsubscribe(_keyEventsChannel);
        }

        public void Dispose()
        {
            Stop();
        }

        
        private void StartListening()
        {
            _redisConnectionMultiplexer
                .GetSubscriber()
                .Subscribe(
                    _keyEventsChannel,
                    (channel, operation) =>
                    {
                        if (_isPaused) return;
                        
                        var key = channel.ToString().Replace(KeyspaceEventsPrefix, "");
                        
                        if (KeysToIgnore.Contains(key)) return;

                        _logger.Info(string.Format("Alteration of the dataset on Redis detected (key: \"{0}\", operation: \"{1}\")", key, operation));

                        if (KeyChanged == null) return;                        
                        
                        KeyChanged(channel, operation);
                    }
                );        
        }
    }
}
