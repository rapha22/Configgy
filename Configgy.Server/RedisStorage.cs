using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Configgy.Server
{
    internal class RedisStorage : IDisposable
    {
        private ConnectionMultiplexer _redisConnectionMultiplexer;
        private RedisStorageMonitor _monitor;
        private RedisKeyBuilder _keyBuilder;
        private int _pulseCheckInterval;
        private ILogger _logger;

        public RedisStorage(string redisConnectionString, ILogger logger, string prefix = null, int pulseCheckInterval = 5000)
        {
            if (redisConnectionString == null) throw new ArgumentNullException("redisConnectionString");

            var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
            redisOptions.AllowAdmin = true;

            _redisConnectionMultiplexer = ConnectionMultiplexer.Connect(redisOptions);
            _keyBuilder = new RedisKeyBuilder(prefix);
            _pulseCheckInterval = pulseCheckInterval;
            _logger = logger;
        }

        public void UploadConfigurationSpace(IDictionary<string, object> configurationSpace)
        {
            if (_monitor != null)
            {
                _monitor.PauseMonitoringFor(() => UploadConfigurationSpaceInternal(configurationSpace));
            }
            else
            {
                UploadConfigurationSpaceInternal(configurationSpace);
            }
        }

        public void MonitorChanges(Action actionOnChange)
        {
            _monitor = new RedisStorageMonitor(_redisConnectionMultiplexer, _keyBuilder, _logger, _pulseCheckInterval);
            _monitor.DataSetChanged += actionOnChange;
            _monitor.Start();
        }

        public void Dispose()
        {
            _monitor.Dispose();
            _redisConnectionMultiplexer.Dispose();
        }


        private void UploadConfigurationSpaceInternal(IDictionary<string, object> configurationSpace)
        {
            var redis = _redisConnectionMultiplexer.GetDatabase();

            DeleteKeys(pattern: _keyBuilder.BuildKey("*"));

            foreach (var entry in configurationSpace)
            {
                var redisKey = _keyBuilder.BuildKey(entry.Key);

                redis.StringSet(redisKey, JsonConvert.SerializeObject(entry.Value));
            }
        }

        private void DeleteKeys(string pattern)
        {
            foreach (var server in _redisConnectionMultiplexer.GetEndPoints())
            {
                var redisDatabase = _redisConnectionMultiplexer.GetDatabase();
                var redisServer = _redisConnectionMultiplexer.GetServer(server);

                foreach (var key in redisServer.Keys(pattern: pattern))
                {
                    redisDatabase.KeyDelete(key);
                }
            }
        }
    }
}
