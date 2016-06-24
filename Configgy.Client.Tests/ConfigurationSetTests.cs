using Xunit;

namespace Configgy.Client.Tests
{
    public class ConfigurationSetTests
    {
        [Fact]
        public void Get_ShouldExposeUnderlyingData()
        {
            var content = @"{ ""Prop1"": ""test"", ""Prop2"": { Name: ""terrible!""} }";
            var set = new ConfigurationSet(content, new JsonConfigurationSetParser());

            dynamic underlyingData = set.Get();

            Assert.Equal("test", (string)underlyingData.Prop1);
            Assert.Equal("terrible!", (string)underlyingData.Prop2.Name);
        }
    }
}
