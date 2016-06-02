using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using StackExchange.Redis;

namespace Configgy.Server
{
    public class RedisStorageMonitor : IDisposable
    {
        private const int DefaultPulseCheckInterval = 5000;
        private const int DefaultEventDelayingTime = 1000;

        private ConnectionMultiplexer _redisConnectionMultiplexer;
        private RedisKeyBuilder _keyBuilder;

        private readonly RedisChannel _channel;
        private readonly string _pulseKey;
        private Timer _pulseChecker;
        private Action _actionOnChange;
        private EventDelayer _eventDelayer;

        public RedisStorageMonitor(
            ConnectionMultiplexer redisConnectionMultiplexer,
            RedisKeyBuilder keyBuilder,
            Action actionOnChange,
            int pulseCheckIntervalMs = DefaultPulseCheckInterval,
            int eventDelayingMs = DefaultEventDelayingTime
        )
        {
            _redisConnectionMultiplexer = redisConnectionMultiplexer;
            _keyBuilder = keyBuilder;
            _actionOnChange = actionOnChange;

            _pulseKey = _keyBuilder.BuildKey("pulse");
            _pulseChecker = CreatePulseChecker(pulseCheckIntervalMs);

            var channelPattern = "__keyspace@0__:" + _keyBuilder.BuildKey("*");
            _channel = new RedisChannel(channelPattern, RedisChannel.PatternMode.Pattern);

            _eventDelayer = new EventDelayer(eventDelayingMs, false);
        }

        public void Start()
        {
            StartPulseChecker();

            _redisConnectionMultiplexer
                .GetSubscriber()
                .Subscribe(
                    _channel,
                    (channel, value) => _eventDelayer.Trigger(_actionOnChange)
                );
        }

        public void PauseMonitoringFor(Action action)
        {
            Stop();
            action();
            Start();
        }

        private void Stop()
        {
            _redisConnectionMultiplexer.GetSubscriber().Unsubscribe(_channel);
            _pulseChecker.Stop();
        }

        private Timer CreatePulseChecker(int checkingInterval)
        {
            var pulseChecker = new Timer(checkingInterval);
            pulseChecker.Elapsed += (a, b) =>
            {
                var redis = _redisConnectionMultiplexer.GetDatabase();

                if (!redis.KeyExists(_pulseKey))
                {
                    _eventDelayer.Trigger(_actionOnChange);
                }
            };

            return pulseChecker;
        }

        internal void StartPulseChecker()
        {
            _redisConnectionMultiplexer.GetDatabase().StringSet(_pulseKey, true);
            _pulseChecker.Start();
        }

        public void Dispose()
        {
            Stop();
            _pulseChecker.Dispose();
        }
    }
}
