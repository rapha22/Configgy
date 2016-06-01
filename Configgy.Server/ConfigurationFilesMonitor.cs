using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configgy.Server
{
    internal class ConfigurationFilesMonitor : IDisposable
    {
        private string _basePath;
        private FileSystemWatcher _watcher;

        public bool IsMonitoring
        {
            get { return _watcher.EnableRaisingEvents; }
        }

        public ConfigurationFilesMonitor(string basePath, string filesFilter)
        {
            if (string.IsNullOrEmpty(basePath)) throw new ArgumentException("The basePath must be provided.", "basePath");
            if (string.IsNullOrEmpty(filesFilter)) throw new ArgumentException("The filesFilter must be provided.", "filesFilter");

            _basePath = basePath;

            _watcher = new FileSystemWatcher(basePath, filesFilter)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size
            };
        }

        public void MonitorChanges(Action actionOnChange)
        {
            if (_watcher.EnableRaisingEvents)
                throw new InvalidOperationException("The watcher is already monitoring changes.");

            _watcher.Created += (_, __) => actionOnChange();
            _watcher.Changed += (_, __) => actionOnChange();
            _watcher.Renamed += (_, __) => actionOnChange();
            _watcher.Deleted += (_, __) => actionOnChange();

            _watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }
    }
}
