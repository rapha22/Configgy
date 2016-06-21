using System;

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
            _redisStorage = new RedisStorage(options.RedisConnectionString, logger, options.Prefix);
            _filesMonitor = new ConfigurationFilesMonitor(options.ConfigurationFilesDirectory, options.FilesFilter, logger);
            _logger = logger;

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

            _redisStorage.Dispose();
            _filesMonitor.Dispose();
        }


        private void BuildConfigurationSpace()
        {
            _logger.Info("Starting configuration space build");

            try
            {
                var baseConfigurationSpace = _configSource.GetBaseConfigurationSpace();
                var cs = _merger.CreateMergedConfigurationSpace(baseConfigurationSpace);
                _redisStorage.UploadConfigurationSpace(cs);

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
    }
}
