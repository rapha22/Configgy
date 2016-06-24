using System;
using System.Collections.Concurrent;

namespace Configgy.Client
{
    internal class CachedConfigurationSetParser : IConfigurationSetParser
    {
        private IConfigurationSetParser _parser;
        private ConcurrentDictionary<Tuple<string, Type>, object> _cache = new ConcurrentDictionary<Tuple<string, Type>, object>();

        public CachedConfigurationSetParser(IConfigurationSetParser parser, IServerMonitor serverMonitor)
        {
            _parser = parser;
            serverMonitor.ConfigurationSpaceRebuilt += () => _cache.Clear();
        }

        public dynamic Parse(string content)
        {
            return _cache.GetOrAdd(
                Tuple.Create(null as string, null as Type),
                k => _parser.Parse(content)
            );
        }

        public T Parse<T>(string content, string propertyPath = null)
        {
            return (T)_cache.GetOrAdd(
                Tuple.Create(propertyPath, typeof(T)),
                k => _parser.Parse<T>(content, propertyPath)
            );
        }
    }
}
