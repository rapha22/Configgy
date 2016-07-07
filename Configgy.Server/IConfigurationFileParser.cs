using System.Collections.Generic;

namespace Configgy.Server
{
    interface IConfigurationFileParser
    {
        IDictionary<string, object> Parse(string path);
    }
}
