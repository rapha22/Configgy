using System;
using Configgy.Common;
using StackExchange.Redis;

namespace Configgy.Server
{
    public class RedisStorageMonitor : IMonitor
    {
        private const int DefaultEventDelayingTime = 1000;

        private RedisStorageChangeMonitor _changeMonitor;
        private RedisStoragePulseMonitor _pulseMonitor;

        public event ChangeDetectedHandler ChangeDetected;

        public RedisStorageMonitor(
            ConnectionMultiplexer redisConnectionMultiplexer,
            RedisKeyBuilder keyBuilder,
            ILogger logger
        )
        {
            _pulseMonitor = new RedisStoragePulseMonitor(redisConnectionMultiplexer, keyBuilder);
            _changeMonitor = new RedisStorageChangeMonitor(redisConnectionMultiplexer, keyBuilder, logger);
            _changeMonitor.KeysToIgnore.Add(_pulseMonitor.PulseKey);

            _changeMonitor.ChangeDetected += Trigger;
            _pulseMonitor.ChangeDetected  += Trigger;
        }

        public void Start()
        {
            _pulseMonitor.Start();
            _changeMonitor.Start();
        }

        public void PauseMonitoringFor(Action action)
        {
            _pulseMonitor.Pause();
            _changeMonitor.Pause();

            action();

            _pulseMonitor.Start();
            _changeMonitor.Start();
        }

        public void Dispose()
        {
            _pulseMonitor.Dispose();
            _changeMonitor.Dispose();
        }

        private void Trigger(IMonitor source, string description)
        {
            if (ChangeDetected != null)
                ChangeDetected(this, description);
        }
    }
}
