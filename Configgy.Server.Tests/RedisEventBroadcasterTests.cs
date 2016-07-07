using System.Threading.Tasks;
using Configgy.Common;
using Configgy.TestUtilities;
using StackExchange.Redis;
using Xunit;

namespace Configgy.Server.Tests
{
    public class RedisEventBroadcasterTests
    {
        [Fact]
        public void BroadcastConfigurationSpaceRebuilt()
        {
            var redis = ConnectionMultiplexer.Connect("localhost");
            var keyBuilder = new RedisKeyBuilder("RedisEventBroadcasterTests_BroadcastConfigurationSpaceRebuilt");
            var broadcaster = new RedisEventBroadcaster(redis, keyBuilder);

            var expectedChannel = keyBuilder.BuildKey(RedisChannels.ClientMessages);
            var expectedMessage = RedisMessages.ConfigurationSpaceRebuilt;

            string channel = null;
            string message = null;

            Task<bool> triggered;
            var trigger = AsyncHelper.CreateCompletionTaskAction(out triggered);

            redis.GetSubscriber().Subscribe(expectedChannel, (ch, msg) =>
            {
                channel = ch;
                message = msg;
                trigger();
            });

            broadcaster.BroadcastConfigurationSpaceRebuilt();

            triggered.Wait(1000);

            Assert.Equal(expectedChannel, channel);
            Assert.Equal(expectedMessage, message);
        }
    }
}
