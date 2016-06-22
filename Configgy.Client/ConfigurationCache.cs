using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configgy.Client
{
    internal class ConfigurationCache
    {
        private ConcurrentDictionary<string, object> _configs = new ConcurrentDictionary<string, object>();

        public object Get(string key)
        {
            throw new NotImplementedException();
        }
    }
}
