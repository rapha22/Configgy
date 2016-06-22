using System;
using System.Timers;
using Configgy.Common;
using StackExchange.Redis;

namespace Configgy.Server
{
    public class RedisStoragePulseMonitor : IDisposable
    {
        public const int DefaultPulseCheckInterval = 5000;
        private const string PulseBaseKey = "!_pulse_!";

        private ConnectionMultiplexer _redisConnectionMultiplexer;
        private Timer _timer;
        private ILogger _logger;

        public string PulseKey { get; private set; }
        public event Action PulseMissing;

        public RedisStoragePulseMonitor(ConnectionMultiplexer redisConnectionMultiplexer, RedisKeyBuilder keyBuilder, int checkingIntervalMs, ILogger logger)
        {
            _redisConnectionMultiplexer = redisConnectionMultiplexer;
            PulseKey = keyBuilder.BuildKey(PulseBaseKey);
            _timer = CreatePulseChecker(checkingIntervalMs);
            _logger = logger;
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
                    if (PulseMissing == null) return;

                    var redis = _redisConnectionMultiplexer.GetDatabase();

                    if (!redis.KeyExists(PulseKey))
                    {
                        _logger.Info("Pulse not detected on Redis");

                        if (PulseMissing != null)
                            PulseMissing();
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
