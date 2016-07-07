using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Configgy.Server
{
    internal class FileSystemConfigurationSource : IConfigurationSource
    {
        private string _baseDirectory;
        private string _filesFilter;
        private IConfigurationFileParser _parser;
        private ILogger _logger;

        public FileSystemConfigurationSource(string baseDirectory, string filesFilter, IConfigurationFileParser parser, ILogger logger)
        {
            if (baseDirectory == null) throw new ArgumentNullException("baseDirectory");
            if (filesFilter == null) throw new ArgumentNullException("filesFilter");
            if (parser == null) throw new ArgumentNullException("parser");

            _filesFilter = filesFilter;
            _parser = parser;
            _logger = logger;

            try
            {
                _baseDirectory = Path.GetFullPath(baseDirectory);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid path \"" + baseDirectory + "\"", "baseDirectory", ex);
            }
        }

        public IDictionary<string, object> GetBaseConfigurationSpace()
        {
            return ReadDirectory(_baseDirectory);
        }

        internal IDictionary<string, object> ReadDirectory(string directory)
        {
            directory = directory ?? _baseDirectory;

            var result = new Dictionary<string, object>();

            var files = Directory.EnumerateFiles(directory, _filesFilter, SearchOption.AllDirectories);

            foreach (var f in files)
            {
                _logger.Info("Reading file " + f);
                result.Add(GetConfigurationSetKey(f), _parser.Parse(f));
            }

            _logger.Info("Done reading files");

            return result;
        }

        internal string GetConfigurationSetKey(string configFilePath)
        {
            var fullPath = Path.GetFullPath(configFilePath);

            return
                Path.ChangeExtension(fullPath, "")
                .Replace(_baseDirectory, "")
                .TrimStart(Path.DirectorySeparatorChar)
                .TrimEnd('.')
                .Replace(Path.DirectorySeparatorChar, '/');
        }
    }
}