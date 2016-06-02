using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Configgy.Server
{
    internal class JsonFileConfigurationSource
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
                throw new ArgumentException("The base directory must be a valid absolute or relative path.", "baseDirectory", ex);
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
                result.Add(GetConfigurationSetKey(f), ParseFile(f));
            }

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

        /// <summary>
        /// Creates a relative path from one file or folder to another (see http://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path).
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        private String MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (string.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}
