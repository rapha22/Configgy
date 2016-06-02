using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Configgy.Server.Tests
{
    public class RedisKeyBuilderTests
    {
        [Theory]
        [InlineData("my_prefix")]
        [InlineData("blabla")]
        [InlineData("oh_yeah!")]
        public void BuildKey_WithPrefix(string prefix)
        {
            const string baseKey = "test";

            var keyBuilder = new RedisKeyBuilder(prefix);
            
            var generatedKey = keyBuilder.BuildKey(baseKey);
            var expectedKey = RedisKeyBuilder.GlobalPrefix + RedisKeyBuilder.PrefixSeparator + prefix + RedisKeyBuilder.PrefixSeparator + baseKey;

            Assert.Equal(expectedKey, generatedKey);
        }

        [Fact]
        public void BuildKey_WithNullPrefix()
        {
            const string baseKey = "test";

            var keyBuilder = new RedisKeyBuilder();

            var generatedKey = keyBuilder.BuildKey(baseKey);
            var expectedKey = RedisKeyBuilder.GlobalPrefix + RedisKeyBuilder.PrefixSeparator + baseKey;

            Assert.Equal(expectedKey, generatedKey);
        }
    }
}
