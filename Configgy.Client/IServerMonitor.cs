using System;

namespace Configgy.Client
{
    public interface IServerMonitor
    {
        void Start();
        event Action ConfigurationSpaceRebuilt;
    }
}
