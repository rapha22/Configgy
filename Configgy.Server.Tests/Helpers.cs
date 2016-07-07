using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configgy.Server.Tests
{
    public static class MonitorTestHelper
    {
        public static ChangeDetectedHandler CreateCompletionTaskAction(out Task<bool> task)
        {
            var tcs = new TaskCompletionSource<bool>();
            task = tcs.Task;
            return (_, __) => tcs.SetResult(true);
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
}
