using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Configgy.Server
{
    public class JsonConfigurationFileParser : IConfigurationFileParser
    {
        private ILogger _logger;

        public JsonConfigurationFileParser(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;
        }

        public IDictionary<string, object> Parse(string path)
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
    }
}
