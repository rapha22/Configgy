using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Configgy.Client.Tests
{
    public class ConfiggyClientTests
    {
        private ConnectionMultiplexer redisHub;

        public ConfiggyClientTests()
        {
            redisHub = ConnectionMultiplexer.Connect("localhost");
        }

        
    }
}
