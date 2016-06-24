using System;
using System.Collections.Generic;

namespace Configgy.Client.Tests
{
    public class StubConfigurationSpaceStorage : IConfigurationSpaceStorage
    {
        private IDictionary<string, string> _dataSource;

        public int AccessCount { get; private set; }

        public StubConfigurationSpaceStorage(IDictionary<string, string> dataSource)
        {
            _dataSource = dataSource;
        }

        public string Get(string key)
        {
            AccessCount++;
            return _dataSource[key];
        }
    }

    public class StubServerMonitor : IServerMonitor
    {
        public void TriggerConfigurationSpaceRebuilt()
        {
            if (ConfigurationSpaceRebuilt != null)
                ConfigurationSpaceRebuilt();
        }

        public void Start() { }

        public event Action ConfigurationSpaceRebuilt;
    }

    public class StubConfigurationSetParser : IConfigurationSetParser
    {
        public int AccessCount { get; private set; }

        public dynamic Parse(string content)
        {
            AccessCount++;
            return content;
        }

        public T Parse<T>(string content, string propertyPath = null)
        {
            AccessCount++;
            return (T)Convert.ChangeType(content, typeof(T));
        }
    }
}
