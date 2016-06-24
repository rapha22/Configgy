using Configgy.Common;
using StackExchange.Redis;
using Xunit;

namespace Configgy.Client.Tests
{
    public class RedisConfigurationSpaceStorageTests
    {
        [Fact]
        public void Get_ShouldReturnTheContentOnAGivenKey()
        {
            const string expectedContent = "content";
            const string key = "test_key";

            var redisHub = ConnectionMultiplexer.Connect("localhost");
            var keyBuilder = new RedisKeyBuilder("RedisConfigurationSpaceStorageTests");
            var storage = new RedisConfigurationSpaceStorage(redisHub, keyBuilder);

            redisHub.GetDatabase().StringSet(keyBuilder.BuildKey(key), expectedContent);

            var content = storage.Get(key);

            Assert.Equal(expectedContent, content);
        }
    }
}
