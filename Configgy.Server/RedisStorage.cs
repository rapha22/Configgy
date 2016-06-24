using System;
using System.Collections.Generic;
using Configgy.Common;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Configgy.Server
{
    internal class RedisStorage : IStorage
    {
        private ConnectionMultiplexer _connectionMultiplexer;
        private RedisStorageMonitor _monitor;
        private RedisKeyBuilder _keyBuilder;
        private ILogger _logger;

        public RedisStorage(ConnectionMultiplexer connectionMultiplexer, RedisKeyBuilder keyBuilder, RedisStorageMonitor monitor)
        {
            if (connectionMultiplexer == null) throw new ArgumentNullException("connectionMultiplexer");
            if (keyBuilder == null) throw new ArgumentNullException("keyBuilder");
            if (monitor == null) throw new ArgumentNullException("monitor");

            _connectionMultiplexer = connectionMultiplexer;
            _keyBuilder = keyBuilder;
            _monitor = monitor;
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

        public void Dispose()
        {
            _monitor.Dispose();
            _connectionMultiplexer.Dispose();
        }


        private void UploadConfigurationSpaceInternal(IDictionary<string, object> configurationSpace)
        {
            var redis = _connectionMultiplexer.GetDatabase();

            DeleteKeys(pattern: _keyBuilder.BuildKey("*"));

            foreach (var entry in configurationSpace)
            {
                var redisKey = _keyBuilder.BuildKey(entry.Key);

                redis.StringSet(redisKey, JsonConvert.SerializeObject(entry.Value));
            }
        }

        private void DeleteKeys(string pattern)
        {
            foreach (var server in _connectionMultiplexer.GetEndPoints())
            {
                var redisDatabase = _connectionMultiplexer.GetDatabase();
                var redisServer = _connectionMultiplexer.GetServer(server);

                foreach (var key in redisServer.Keys(pattern: pattern))
                {
                    redisDatabase.KeyDelete(key);
                }
            }
        }
    }
}
