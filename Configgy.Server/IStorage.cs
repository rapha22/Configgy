using System;
using System.Collections.Generic;

namespace Configgy.Server
{
    public interface IStorage : IDisposable
    {
        void UploadConfigurationSpace(IDictionary<string, object> configurationSpace);
    }
}
