using System.Collections.Generic;

namespace Configgy.Server
{
    public interface IConfigurationSource
    {
        IDictionary<string, object> GetBaseConfigurationSpace();
    }
}
