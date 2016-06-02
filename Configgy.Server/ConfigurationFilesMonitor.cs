using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Configgy.Server
{
    internal class ConfigurationFilesMonitor : IDisposable
    {
        private const int DefaultEventDelayTime = 1000;

        private string _basePath;
        private FileSystemWatcher _watcher;
        private EventDelayer _eventDelayer;

        public bool IsMonitoring
        {
            get { return _watcher.EnableRaisingEvents; }
        }

        public ConfigurationFilesMonitor(string basePath, string filesFilter, int eventDelayingMs = DefaultEventDelayTime)
        {
            if (string.IsNullOrEmpty(basePath)) throw new ArgumentException("The basePath must be provided.", "basePath");
            if (string.IsNullOrEmpty(filesFilter)) throw new ArgumentException("The filesFilter must be provided.", "filesFilter");

            _basePath = basePath;

            _watcher = new FileSystemWatcher(basePath, filesFilter)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size
            };

            _eventDelayer = new EventDelayer(eventDelayingMs, true);
        }

        public void MonitorChanges(Action actionOnChange)
        {
            if (_watcher.EnableRaisingEvents)
                throw new InvalidOperationException("The watcher is already monitoring changes.");

            _watcher.Created += (_, __) => _eventDelayer.Trigger(actionOnChange);
            _watcher.Changed += (_, __) => _eventDelayer.Trigger(actionOnChange);
            _watcher.Renamed += (_, __) => _eventDelayer.Trigger(actionOnChange);
            _watcher.Deleted += (_, __) => _eventDelayer.Trigger(actionOnChange);

            _watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }
    }
}
