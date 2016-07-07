using System;
using System.Collections.Generic;

namespace Configgy.Server.Tests
{
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

    public class StubConfigurationFileParser : IConfigurationFileParser
    {
        public IDictionary<string, object> Parse(string path)
        {
            return new Dictionary<string, object>();
        }
    }
}
