using System;
using System.Threading.Tasks;
using Configgy.Common;
using StackExchange.Redis;
using Xunit;

namespace Configgy.Client.Tests
{
    public class RedisServerMonitorTests
    {
        public class ServerMonitorTest : IDisposable
        {
            protected ConnectionMultiplexer redisHub;
            protected RedisKeyBuilder keyBuilder;

            public ServerMonitorTest()
            {
                redisHub = ConnectionMultiplexer.Connect("localhost,allowAdmin=true");
                keyBuilder = new RedisKeyBuilder("server_monitor_tests");
            }

            public virtual void Dispose()
            {
                redisHub.Dispose();
            }

            
            private void PublishRedisMessage(string message)
            {
                var channel = keyBuilder.BuildKey(RedisChannels.ClientMessages);
                redisHub.GetDatabase().Publish(channel, message);
            }


            [Fact]
            public void OnReceiving_ConfigurationSpaceRebuildMessage_ShouldTrigger()
            {
                Task<bool> triggered;

                var monitor = new RedisServerMonitor(redisHub, new RedisKeyBuilder("server_monitor_tests"));
                monitor.Start();
                monitor.ConfigurationSpaceRebuilt += AsyncHelper.CreateCompletionTaskAction(out triggered);

                PublishRedisMessage(RedisMessages.ConfigurationSpaceRebuilt);

                Assert.True(triggered.Result);
            }

            [Fact]
            public void OnReceiving_UnknownMessage_ShouldNotTrigger()
            {
                var triggered = false;

                var monitor = new RedisServerMonitor(redisHub, new RedisKeyBuilder("server_monitor_tests"));
                monitor.Start();
                monitor.ConfigurationSpaceRebuilt += () => triggered = true;

                PublishRedisMessage("???????");

                Task.Delay(200).Wait();

                Assert.False(triggered);
            }
        }
    }
}
