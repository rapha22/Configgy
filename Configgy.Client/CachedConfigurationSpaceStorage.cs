using System.Collections.Concurrent;

namespace Configgy.Client
{
    internal class CachedConfigurationSpaceStorage : IConfigurationSpaceStorage
    {
        private IConfigurationSpaceStorage _underlyingStorage;
        private ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();

        internal CachedConfigurationSpaceStorage(IConfigurationSpaceStorage storage, IServerMonitor serverMonitor)
        {
            _underlyingStorage = storage;
            serverMonitor.ConfigurationSpaceRebuilt += OnConfigurationSpaceRebuilt;
        }

        public string Get(string key)
        {
            return _cache.GetOrAdd(key, _underlyingStorage.Get);
        }

        private void OnConfigurationSpaceRebuilt()
        {
            _cache.Clear();
        }
    }
}
