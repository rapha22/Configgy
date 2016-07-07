using System.Linq;
using Configgy.TestUtilities;
using Xunit;

namespace Configgy.Server.Tests
{
    public class JsonConfigurationFileParserTests
    {
        [Fact]
        public void ParseFile_WhenFileIsNotEmpty_ShouldNotReturnNull()
        {
            var jsonFileReader = new JsonConfigurationFileParser(new StubLogger());
            var configurationFilePath = @"test_files\config1.json".ToAbsolutePath();

            var configurationSet = jsonFileReader.Parse(configurationFilePath);

            Assert.NotNull(configurationSet);
            Assert.NotNull(configurationSet.Values.First());
        }

        [Fact]
        public void ParseFile_WhenFileIsEmpty_ShouldNotReturnNull()
        {
            var logger = new StubLogger();

            var jsonFileReader = new JsonConfigurationFileParser(logger);
            var configurationFilePath = @"test_files\empty.json".ToAbsolutePath();

            var configurationSet = jsonFileReader.Parse(configurationFilePath);

            Assert.NotNull(configurationSet);
            Assert.Equal(1, logger.Warnings.Count());
        }

        [Fact]
        public void ParseFile_WhenFileIsInvalid_ShouldNotReturnNull()
        {
            var logger = new StubLogger();

            var jsonFileReader = new JsonConfigurationFileParser(logger);
            var configurationFilePath = @"test_files\invalid.json".ToAbsolutePath();

            var configurationSet = jsonFileReader.Parse(configurationFilePath);

            Assert.NotNull(configurationSet);
            Assert.Equal(1, logger.Errors.Count());
        }
    }
}
