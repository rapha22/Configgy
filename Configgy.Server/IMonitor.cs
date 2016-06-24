using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configgy.Server
{
    public delegate void ChangeDetectedHandler(IMonitor source, string description);

    public class ChangeDetectedEventData
    {
        public string Description { get; set; }
        public object Data { get; set; }
    }

    public interface IMonitor : IDisposable
    {
        string Description { get; }
        event ChangeDetectedHandler ChangeDetected;
        void Start();
    }
}
