using System;
using Configgy.Common;
using StackExchange.Redis;

namespace Configgy.Server
{
    public class RedisStorageMonitor : IMonitor
    {
        private RedisStorageChangeMonitor _changeMonitor;
        private RedisStoragePulseMonitor _pulseMonitor;

        public event ChangeDetectedHandler ChangeDetected;

        public RedisStorageMonitor(
            ConnectionMultiplexer redisConnectionMultiplexer,
            RedisKeyBuilder keyBuilder,
            ILogger logger,
            int pulseCheckIntervalMs = RedisStoragePulseMonitor.DefaultPulseCheckInterval
        )
        {
            _pulseMonitor = new RedisStoragePulseMonitor(redisConnectionMultiplexer, keyBuilder, pulseCheckIntervalMs);
            _changeMonitor = new RedisStorageChangeMonitor(redisConnectionMultiplexer, keyBuilder, logger);
            _changeMonitor.KeysToIgnore.Add(_pulseMonitor.PulseKey);

            _changeMonitor.ChangeDetected += (a, b) => ChangeDetected.Trigger(a, b);
            _pulseMonitor.ChangeDetected += (a, b) => ChangeDetected.Trigger(a, b);
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
    }
}
