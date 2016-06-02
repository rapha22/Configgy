using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Configgy.Server.Tests
{
    public class JsonFileConfigurationSourceTests
    {
        public class Constructor
        {
            [Fact]
            public void WhenPassingNullBaseDirectory_ShouldThrow()
            {
                Assert.Throws<ArgumentNullException>(() => new JsonFileConfigurationSource(null, "*.json", new StubLogger()));
            }

            [Fact]
            public void WhenPassingInvalidBaseDirectory_ShouldThrow()
            {
                Assert.Throws<ArgumentException>(() => new JsonFileConfigurationSource("????", "*.json", new StubLogger()));
            }

            [Fact]
            public void WhenPassingNullFilesFilter_ShouldThrow()
            {
                Assert.Throws<ArgumentNullException>(() => new JsonFileConfigurationSource(".", null, new StubLogger()));
            }
        }

        [Fact]
        public void ParseFile_WhenFileIsNotEmpty_ShouldNotReturnNull()
        {
            var jsonFileReader = new JsonFileConfigurationSource(".", "", new StubLogger());
            var configurationFilePath = @"test_files\config1.json".ToAbsolutePath();

            var configurationSet = jsonFileReader.ParseFile(configurationFilePath);

            Assert.NotNull(configurationSet);
            Assert.NotNull(configurationSet.Values.First());
        }

        [Fact]
        public void ParseFile_WhenFileIsEmpty_ShouldNotReturnNull()
        {
            var logger = new StubLogger();

            var jsonFileReader = new JsonFileConfigurationSource(".", "", logger);
            var configurationFilePath = @"test_files\empty.json".ToAbsolutePath();

            var configurationSet = jsonFileReader.ParseFile(configurationFilePath);

            Assert.NotNull(configurationSet);
            Assert.Equal(1, logger.Warnings.Count());
        }

        [Fact]
        public void ParseFile_WhenFileIsInvalid_ShouldNotReturnNull()
        {
            var logger = new StubLogger();

            var jsonFileReader = new JsonFileConfigurationSource(".", "", logger);
            var configurationFilePath = @"test_files\invalid.json".ToAbsolutePath();

            var configurationSet = jsonFileReader.ParseFile(configurationFilePath);

            Assert.NotNull(configurationSet);
            Assert.Equal(1, logger.Errors.Count());
        }

        public class GetConfigurationSetKey
        {
            const string expectedKey = "second/file";

            [Fact]
            public void BaseDirectoryWithTrailingBackslash()
            {
                var jsonFileReader = new JsonFileConfigurationSource(@"C:\first\", "", new StubLogger());

                var configurationSetKey = jsonFileReader.GetConfigurationSetKey(@"C:\first\second\file.txt");

                Assert.Equal(expectedKey, configurationSetKey);
            }

            [Fact]
            public void BaseDirectoryWithoutTrailingBackslash()
            {
                var jsonFileReader = new JsonFileConfigurationSource(@"C:\first", "", new StubLogger());

                var configurationSetKey = jsonFileReader.GetConfigurationSetKey(@"C:\first\second\file.txt");

                Assert.Equal(expectedKey, configurationSetKey);
            }

            [Fact]
            public void BaseDirectoryWithAlternativaFilePathSeparator()
            {
                var jsonFileReader = new JsonFileConfigurationSource(@"C:/first", "", new StubLogger());

                var configurationSetKey = jsonFileReader.GetConfigurationSetKey(@"C:/first/second/file.txt");

                Assert.Equal(expectedKey, configurationSetKey);
            }
        }

        [Fact]
        public void GetBaseConfigurationSpace()
        {
            const string filesFilter = "*.json";

            var baseDirectory = @"test_files\".ToAbsolutePath();
            var jsonFileReader = new JsonFileConfigurationSource(baseDirectory, filesFilter, new StubLogger());
            var numberOfFiles = Directory.EnumerateFiles(baseDirectory, filesFilter, SearchOption.AllDirectories).Count();

            var configurationSets = jsonFileReader.GetBaseConfigurationSpace();

            Assert.Equal(numberOfFiles, configurationSets.Count());
            
            //Should have created dictionaries with the correct values
            var set = configurationSets["config1"] as IDictionary<string, object>;
            Assert.Equal("Dollynho, Seu Amiguinho", set["name"]);
            Assert.Equal(3000L, set["age"]);
        }
    }
}
