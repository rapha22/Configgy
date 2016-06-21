using System;
using StackExchange.Redis;

namespace Configgy.Server
{
    public class RedisStorageMonitor : IDisposable
    {
        private const int DefaultEventDelayingTime = 1000;

        private RedisStorageChangeMonitor _changeMonitor;
        private RedisStoragePulseMonitor _pulseMonitor;
        private EventDelayer _eventDelayer;

        public event Action DataSetChanged;

        public RedisStorageMonitor(
            ConnectionMultiplexer redisConnectionMultiplexer,
            RedisKeyBuilder keyBuilder,
            ILogger logger,
            int pulseCheckIntervalMs = RedisStoragePulseMonitor.DefaultPulseCheckInterval,
            int eventDelayingMs = DefaultEventDelayingTime
        )
        {
            _pulseMonitor = new RedisStoragePulseMonitor(redisConnectionMultiplexer, keyBuilder, pulseCheckIntervalMs, logger);
            _changeMonitor = new RedisStorageChangeMonitor(redisConnectionMultiplexer, keyBuilder, logger);
            _changeMonitor.KeysToIgnore.Add(_pulseMonitor.PulseKey);

            Action triggerWithDelay = () => _eventDelayer.Trigger(this.Trigger);

            _changeMonitor.KeyChanged += (_, __) => triggerWithDelay();
            _pulseMonitor.PulseMissing += () => triggerWithDelay();

            _eventDelayer = new EventDelayer(eventDelayingMs, false);
        }

        public void Start()
        {
            _pulseMonitor.Start(); //The pulse monitor must start first, or else the pulse key will trigger the changes monitor
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

        private void Trigger()
        {
            if (DataSetChanged != null)
                DataSetChanged();
        }
    }
}
