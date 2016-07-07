using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configgy.Server
{
    public delegate void ChangeDetectedHandler(IMonitor source, ChangeDetectedEventData eventData);

    public static class ChangeDetectedHandlerExtensions
    {
        public static void Trigger(this ChangeDetectedHandler handler, IMonitor source, ChangeDetectedEventData eventData)
        {
            if (handler != null) handler(source, eventData);
        }
    }

    public class ChangeDetectedEventData
    {
        public string Description { get; set; }

        public ChangeDetectedEventData(string description = null)
        {
            Description = description;
        }
    }

    public interface IMonitor : IDisposable
    {
        event ChangeDetectedHandler ChangeDetected;
        void Start();
    }
}
