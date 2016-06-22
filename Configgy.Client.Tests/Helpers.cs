using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Configgy.Client.Tests
{
    public class AsyncHelper
    {
        public static Action CreateCompletionTaskAction(out Task<bool> task)
        {
            var tcs = new TaskCompletionSource<bool>();
            task = tcs.Task;
            return () => tcs.SetResult(true);
        }

        public static Task<bool> CreateCompletionTaskFor(Action<Action> method)
        {
            var tcs = new TaskCompletionSource<bool>();
            method(() => tcs.SetResult(true));
            return tcs.Task;
        }

        public static Task<bool> CreateCompletionTaskFor(Action<int, Action> method, int ms)
        {
            var tcs = new TaskCompletionSource<bool>();
            method(ms, () => tcs.SetResult(true));
            return tcs.Task;
        }
    }

    public class StubConfigurationSpaceStorage : IConfigurationSpaceStorage
    {
        private IDictionary<string, object> _dataSource;

        public int AccessCount { get; private set; }

        public StubConfigurationSpaceStorage(IDictionary<string, object> dataSource)
        {
            _dataSource = dataSource;
        }

        public object Get(string key)
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
}
