using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Configgy.Server
{
    public class ConfiggyServer : IDisposable
    {
        private ConfiggyServerOptions _options;
        private JsonFileConfigurationSource _configSource;
        private ConfigurationSpaceMerger _merger;
        private RedisStorage _redisStorage;
        private ConfigurationFilesMonitor _filesMonitor;
        private ILogger _logger;
        private bool _started = false;

        public ConfiggyServer(ConfiggyServerOptions options, ILogger logger)
        {
            _options = options;
            _configSource = new JsonFileConfigurationSource(options.ConfigurationFilesDirectory, options.FilesFilter, logger);
            _merger = new ConfigurationSpaceMerger();
            _redisStorage = new RedisStorage(options.RedisConnectionString, options.Prefix);
            _filesMonitor = new ConfigurationFilesMonitor(options.ConfigurationFilesDirectory, options.FilesFilter);
            _logger = logger;
        }

        public void Start()
        {
            lock (this)
            {
                if (_started) throw new InvalidOperationException("The server is already started.");
                _started = true;
            }

            _logger.Info("Starting monitoring");

            BuildConfigurationSpace();
            MonitorChanges();
        }

        public void BuildConfigurationSpace()
        {
            _logger.Info("Starting configuration space build");

            var baseConfigurationSpace = _configSource.GetBaseConfigurationSpace();
            var cs = _merger.CreateMergedConfigurationSpace(baseConfigurationSpace);
            _redisStorage.UploadConfigurationSpace(cs);

            _logger.Info("Configuration space building done");
        }

        internal void MonitorChanges()
        {
            _redisStorage.MonitorChanges(() =>
            {
                _logger.Info("Changes on Redis dataset detected");
                BuildConfigurationSpace();
            });
            
            _filesMonitor.MonitorChanges(() =>
            {
                _logger.Info("File changes detected");
                BuildConfigurationSpace();
            });
        }

        public void Dispose()
        {
            _logger.Info("Disposing Configgy server");

            _redisStorage.Dispose();
            _filesMonitor.Dispose();
        }
    }
}
