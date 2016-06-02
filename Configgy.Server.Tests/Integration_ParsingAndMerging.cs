using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Configgy.Server.Tests
{
    public class Integration_ParsingAndMergingTests
    {
        [Fact]
        public void WhenParsingAndMergingConfigurationFiles()
        {
            var baseDirectory         = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_files");
            var numberOfFiles         = Directory.EnumerateFiles(baseDirectory, "*.json", SearchOption.AllDirectories).Count();
            var fileReader            = new JsonFileConfigurationSource(baseDirectory, "*.json", new StubLogger());
            var rawConfigurationSpace = fileReader.GetBaseConfigurationSpace(); ;
            var merger                = new ConfigurationSpaceMerger();
            
            var configurationSpace = merger.CreateMergedConfigurationSpace(rawConfigurationSpace);

            Assert.Equal(numberOfFiles, configurationSpace.Count); //Should have as many entries as files
        }
    }
}
