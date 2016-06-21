using System;

namespace Configgy.Server
{
    [Serializable]
    public class ConfiggyException : Exception
    {
        public ConfiggyException() { }
        public ConfiggyException(string message) : base(message) { }
        public ConfiggyException(string message, Exception inner) : base(message, inner) { }
        protected ConfiggyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
