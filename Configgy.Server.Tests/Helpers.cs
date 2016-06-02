using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Configgy.Server.Tests
{
    public static class FileHelperExtensions
    {
        public static string ToAbsolutePath(this string relativePath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }
    }

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

    public class RedisHelper
    {
        public static void RemoveKeys(ConnectionMultiplexer redisHub, string keysPattern)
        {
            var server = redisHub.GetServer(redisHub.GetEndPoints()[0]);
            var db = redisHub.GetDatabase();

            foreach (var key in server.Keys(pattern: keysPattern))
            {
                db.KeyDelete(key);
            }
        }
    }

    public class StubLogger : ILogger
    {
        private List<string> _infos = new List<string>();
        private List<string> _warnings = new List<string>();
        private List<Tuple<string, Exception>> _errors = new List<Tuple<string, Exception>>();

        public IEnumerable<string> Infos { get { return _infos; } }
        public IEnumerable<string> Warnings { get { return _warnings; } }
        public IEnumerable<Tuple<string, Exception>> Errors { get { return _errors; } }

        public void Info(string message)
        {
            _infos.Add(message);
        }
        public void Warning(string message)
        {
            _warnings.Add(message);
        }

        public void Error(string message)
        {
            _errors.Add(Tuple.Create(message, null as Exception));
        }

        public void Error(string message, Exception ex)
        {
            _errors.Add(Tuple.Create(message, ex));
        }

        public void Error(Exception ex)
        {
            _errors.Add(Tuple.Create(null as string, ex));
        }
    }

}
