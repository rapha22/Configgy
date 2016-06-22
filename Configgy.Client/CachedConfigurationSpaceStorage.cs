using System.Collections.Concurrent;

namespace Configgy.Client
{
    internal class CachedConfigurationSpaceStorage : IConfigurationSpaceStorage
    {
        private IConfigurationSpaceStorage _underlyingStorage;
        private IServerMonitor _serverMonitor;
        private ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string,object>();

        internal CachedConfigurationSpaceStorage(IConfigurationSpaceStorage storage, IServerMonitor serverMonitor)
        {
            _underlyingStorage = storage;
            _serverMonitor = serverMonitor;

            StartServerMonitor();
        }

        public object Get(string key)
        {
            return _cache.GetOrAdd(key, _underlyingStorage.Get);
        }

        private void StartServerMonitor()
        {
            _serverMonitor.ConfigurationSpaceRebuilt += OnConfigurationSpaceRebuilt;
            _serverMonitor.Start();
        }

        private void OnConfigurationSpaceRebuilt()
        {
            _cache.Clear();
        }
    }
}
