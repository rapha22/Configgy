using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Configgy.Server
{
    internal class JsonFileConfigurationSource : IConfigurationSource
    {
        private string _baseDirectory;
        private string _filesFilter;
        private ILogger _logger;

        public JsonFileConfigurationSource(string baseDirectory, string filesFilter, ILogger logger)
        {
            if (baseDirectory == null) throw new ArgumentNullException("baseDirectory");
            if (filesFilter == null) throw new ArgumentNullException("filesFilter");

            _filesFilter = filesFilter;
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
                result.Add(GetConfigurationSetKey(f), ParseFile(f));
            }

            _logger.Info("Done reading files");

            return result;
        }

        internal IDictionary<string, object> ParseFile(string path)
        {
            using (var reader = new StreamReader(path))
            using (var jsonReader = new JsonTextReader(reader))
            {
                try
                {
                    var configurationSet = new JsonSerializer().Deserialize<Dictionary<string, object>>(jsonReader);

                    if (configurationSet != null) return configurationSet;

                    _logger.Warning("File " + path + " is empty");

                    return new Dictionary<string, object>();
                }
                catch (JsonReaderException ex)
                {
                    _logger.Error("Error reading file " + path + ". Ignoring configuration set contents", ex);
                    return new Dictionary<string, object>();
                }
            }
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
