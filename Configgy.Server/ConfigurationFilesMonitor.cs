using System;
using System.IO;

namespace Configgy.Server
{
    internal class ConfigurationFilesMonitor : IMonitor
    {
        private string _basePath;
        private FileSystemWatcher _watcher;
        private ILogger _logger;

        public event ChangeDetectedHandler ChangeDetected;

        public ConfigurationFilesMonitor(string basePath, string filesFilter, ILogger logger)
        {
            if (string.IsNullOrEmpty(basePath)) throw new ArgumentException("The basePath must be provided.", "basePath");
            if (string.IsNullOrEmpty(filesFilter)) throw new ArgumentException("The filesFilter must be provided.", "filesFilter");

            _basePath = basePath;
            _logger = logger;

            _watcher = new FileSystemWatcher(basePath, filesFilter)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size
            };

            AttachEventsHandlers();
        }

        public void MonitorChanges()
        {
            if (_watcher.EnableRaisingEvents)
                throw new InvalidOperationException("The watcher is already monitoring changes.");

            _watcher.EnableRaisingEvents = true;
        }

        private void AttachEventsHandlers()
        {
            _watcher.Created += Trigger;
            _watcher.Changed += Trigger;
            _watcher.Deleted += Trigger;

            _watcher.Renamed += (_, ev) =>
            {
                if (ChangeDetected != null)
                    ChangeDetected(this, string.Format("File event detected: Renamed {0} to {1} (full path: {2})", ev.OldName, ev.Name, ev.FullPath));
            };        
        }

        private void Trigger(object sender, FileSystemEventArgs ev)
        {
            if (ChangeDetected != null)
                ChangeDetected(this, string.Format("File event detected: {0} {1}", ev.ChangeType, ev.FullPath));
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }
    }
}
