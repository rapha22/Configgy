using System.Collections.Generic;
using Xunit;

namespace Configgy.Client.Tests
{
    public class CachedConfigurationSpaceStorageTests
    {
        [Fact]
        public void ShouldCacheValues()
        {
            var configSpace = new Dictionary<string, string> { { "A", "initial value" } };
            var storage = new StubConfigurationSpaceStorage(configSpace);

            var client = new CachedConfigurationSpaceStorage(storage, new StubServerMonitor());

            client.Get("A");
            client.Get("A");
            client.Get("A");
            client.Get("A");
            client.Get("A");
            client.Get("A");

            Assert.Equal(1, storage.AccessCount);
        }

        [Fact]
        public void OnConfigurationSpaceRebuilt_ShouldRefreshCache()
        {
            const string expectedValue = "new value";

            var configSpace = new Dictionary<string, string> { { "A", "initial value" } };
            var storage = new StubConfigurationSpaceStorage(configSpace);
            var serverMonitor = new StubServerMonitor();

            var client = new CachedConfigurationSpaceStorage(storage, serverMonitor);

            client.Get("A");

            configSpace["A"] = expectedValue;
            serverMonitor.TriggerConfigurationSpaceRebuilt();

            var value = client.Get("A");

            Assert.Equal(expectedValue, value);
            Assert.Equal(2, storage.AccessCount);
        }
    }
}
