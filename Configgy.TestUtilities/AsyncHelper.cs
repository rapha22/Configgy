using System;
using System.Threading.Tasks;

namespace Configgy.TestUtilities
{
    public static class AsyncHelper
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

        public static bool Result(this Task<bool> completionTask, int millisecondsTimeout)
        {
            Task.WaitAny(completionTask, Task.Delay(millisecondsTimeout));
            if (completionTask.IsCompleted)
                return completionTask.Result;
            else
                throw new TimeoutException("The given task took more than " + millisecondsTimeout + "milliseconds to complete.");
        }
    }
}
