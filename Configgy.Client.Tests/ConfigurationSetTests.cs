using Xunit;

namespace Configgy.Client.Tests
{
    public class ConfigurationSetTests
    {
        [Fact]
        public void Get_ShouldExposeUnderlyingData()
        {
            var data = new { Prop1 = "test", Prop2 = new { Name = "terrible!" } };
            var set = new ConfigurationSet(data);

            dynamic underlyingData = set.Get();

            Assert.Equal(data.Prop1, (string)underlyingData.Prop1);
            Assert.Equal(data.Prop2.Name, (string)underlyingData.Prop2.Name);
        }
    }
}
