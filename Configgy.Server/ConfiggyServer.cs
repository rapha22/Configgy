using System;
using System.Collections.Generic;

namespace Configgy.Server
{
    public class ConfiggyServer : IDisposable
    {
        private bool _started = false;
        private EventDelayer _eventDelayer;
        private ConfigurationSpaceMerger _merger;
        private IConfigurationSource _configSource;
        private ConfigurationFilesMonitor _filesMonitor;
        private IStorage _storage;
        private IEventBroadcaster _eventBroadcaster;
        private ILogger _logger;
        private List<IMonitor> _monitors;


        public ConfiggyServer(ConfiggyServerOptions options, ILogger logger)
        {
            _logger       = logger;
            _eventDelayer = new EventDelayer(1000, false);
            _merger       = new ConfigurationSpaceMerger();
            _monitors     = new List<IMonitor>();

            var redisFactory = new RedisStorageResourcesFactory(options.RedisConnectionString, options.Prefix, logger);
            _storage = redisFactory.GetStorage();
            _eventBroadcaster = redisFactory.GetEventBroadcaster();
            _monitors.AddRange(redisFactory.GetMonitors());

            _configSource = new JsonFileConfigurationSource(options.ConfigurationFilesDirectory, options.FilesFilter, logger);
            _filesMonitor = new ConfigurationFilesMonitor(options.ConfigurationFilesDirectory, options.FilesFilter, logger);
            _monitors.Add(_filesMonitor);

            GenericExceptionHandler.Initialize(logger);
        }


        public void Start()
        {
            try
            {
                lock (this)
                {
                    if (_started) throw new InvalidOperationException("The server is already started.");
                    _started = true;
                }

                _logger.Info("Starting monitoring");

                MonitorChanges();
                BuildConfigurationSpace();
            }
            catch (Exception ex)
            {
                GenericExceptionHandler.Handle(
                    new ConfiggyException("Error starting Configgy server", ex)
                );

                throw;
            }
        }

        public void Dispose()
        {
            _logger.Info("Disposing Configgy server");

            _storage.Dispose();
            _filesMonitor.Dispose();
        }


        private void BuildConfigurationSpace()
        {
            _logger.Info("Starting configuration space build");

            try
            {
                var baseConfigurationSpace = _configSource.GetBaseConfigurationSpace();
                var cs = _merger.CreateMergedConfigurationSpace(baseConfigurationSpace);
                _storage.UploadConfigurationSpace(cs);

                _logger.Info("Configuration space building done");


            }
            catch (Exception ex)
            {
                GenericExceptionHandler.Handle(
                    new ConfiggyException("Error building configuration space. Maintaining current build. Please fix the issue for the build to complete.", ex)
                );
            }
        }

        private void MonitorChanges()
        {
            foreach (var monitor in _monitors)
            {
                monitor.ChangeDetected += (source, description) =>
                {
                    _logger.Info(description);
                    _eventDelayer.Trigger(BuildConfigurationSpace);
                };
            }
        }
    }
}
