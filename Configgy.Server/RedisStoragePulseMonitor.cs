using System;
using System.Timers;
using Configgy.Common;
using StackExchange.Redis;

namespace Configgy.Server
{
    public class RedisStoragePulseMonitor : IMonitor
    {
        public const int DefaultPulseCheckInterval = 5000;
        private const string PulseBaseKey = "!_pulse_!";

        private ConnectionMultiplexer _redisConnectionMultiplexer;
        private Timer _timer;

        public string PulseKey { get; private set; }
        
        public event ChangeDetectedHandler ChangeDetected;

        public RedisStoragePulseMonitor(ConnectionMultiplexer redisConnectionMultiplexer, RedisKeyBuilder keyBuilder, int checkingIntervalMs = DefaultPulseCheckInterval)
        {
            _redisConnectionMultiplexer = redisConnectionMultiplexer;
            PulseKey = keyBuilder.BuildKey(PulseBaseKey);
            _timer = CreatePulseChecker(checkingIntervalMs);
        }

        
        public void Start()
        {
            _redisConnectionMultiplexer.GetDatabase().StringSet(PulseKey, true);
            _timer.Start();
        }

        public void Pause()
        {
            _timer.Stop();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }


        private Timer CreatePulseChecker(int checkingInterval)
        {
            var pulseChecker = new Timer(checkingInterval);
            pulseChecker.Elapsed += (a, b) =>
            {
                try
                {
                    if (ChangeDetected == null) return;

                    var redis = _redisConnectionMultiplexer.GetDatabase();

                    if (!redis.KeyExists(PulseKey))
                    {
                        if (ChangeDetected != null)
                            ChangeDetected(this, new ChangeDetectedEventData { Description = "Pulse not detected on Redis" });
                    }
                }
                catch (Exception ex)
                {
                    GenericExceptionHandler.Handle(ex);
                }
            };

            return pulseChecker;
        }
    }
}
