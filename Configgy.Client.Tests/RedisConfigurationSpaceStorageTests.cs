using Configgy.Common;
using StackExchange.Redis;
using Xunit;

namespace Configgy.Client.Tests
{
    public class RedisConfigurationSpaceStorageTests
    {
        [Fact]
        public void Get_ShouldReturnAParsedObject()
        {
            const string json = @"{""id"":3, ""name"":""test""}";
            const string key = "test_key";

            var redisHub = ConnectionMultiplexer.Connect("localhost");
            var keyBuilder = new RedisKeyBuilder("RedisConfigurationSpaceStorageTests");
            var storage = new RedisConfigurationSpaceStorage(redisHub, keyBuilder);

            redisHub.GetDatabase().StringSet(keyBuilder.BuildKey(key), json);

            dynamic value = storage.Get(key);

            Assert.Equal(3, (int)value.id);
            Assert.Equal("test", (string)value.name);
        }
    }
}
