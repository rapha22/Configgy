using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Configgy.Server
{
    public class GenericExceptionHandler
    {
        private static GenericExceptionHandler _instance;
        private BlockingCollection<Exception> _exceptions = new BlockingCollection<Exception>();
        private ILogger _logger;


        internal GenericExceptionHandler(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            _logger = logger;

            Task.Factory.StartNew(HandleExceptions, TaskCreationOptions.LongRunning);
        }

        
        public static void Initialize(ILogger logger)
        {
            if (_instance != null)
                throw new InvalidOperationException("The handler was already initialized.");

            _instance = new GenericExceptionHandler(logger);
        }

        public static void Handle(Exception ex)
        {
            _instance.InternalHandle(ex);
        }


        internal void InternalHandle(Exception ex)
        {
            if (ex != null) _exceptions.Add(ex);
        }

        private void HandleExceptions()
        {
            while (true)
            {
                var ex = _exceptions.Take();
                _logger.Error(ex);
            }
        }
    }
}
