using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Configgy.TestUtilities;
using Xunit;

namespace Configgy.Server.Tests
{
    public class FileSystemConfigurationSourceTests
    {
        public class GetConfigurationSetKey
        {
            const string expectedKey = "second/file";

            [Fact]
            public void BaseDirectoryWithTrailingBackslash()
            {
                var jsonFileReader = new FileSystemConfigurationSource(@"C:\first\", "", new StubConfigurationFileParser(), new StubLogger());

                var configurationSetKey = jsonFileReader.GetConfigurationSetKey(@"C:\first\second\file.txt");

                Assert.Equal(expectedKey, configurationSetKey);
            }

            [Fact]
            public void BaseDirectoryWithoutTrailingBackslash()
            {
                var jsonFileReader = new FileSystemConfigurationSource(@"C:\first", "", new StubConfigurationFileParser(), new StubLogger());

                var configurationSetKey = jsonFileReader.GetConfigurationSetKey(@"C:\first\second\file.txt");

                Assert.Equal(expectedKey, configurationSetKey);
            }

            [Fact]
            public void BaseDirectoryWithAlternativaFilePathSeparator()
            {
                var jsonFileReader = new FileSystemConfigurationSource(@"C:/first", "", new StubConfigurationFileParser(), new StubLogger());

                var configurationSetKey = jsonFileReader.GetConfigurationSetKey(@"C:/first/second/file.txt");

                Assert.Equal(expectedKey, configurationSetKey);
            }
        }

        [Fact]
        public void GetBaseConfigurationSpace()
        {
            const string filesFilter = "*.json";

            var baseDirectory = @"test_files\".ToAbsolutePath();
            var logger = new StubLogger();
            var jsonFileReader = new FileSystemConfigurationSource(baseDirectory, filesFilter, new JsonConfigurationFileParser(logger), logger);
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
