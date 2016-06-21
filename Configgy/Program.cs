using System;
using System.Threading;
using System.Threading.Tasks;
using Configgy.Server;

namespace Configgy
{
    class Program
    {
        static ConfiggyServer _server;

        static void Main(string[] args)
        {
            var logger = new ConsoleLogger();

            try
            {
                ShowTitle();

                var options = Configuration.GetOptions(args);

                _server = new ConfiggyServer(options, logger);

                Task.Factory.StartNew(
                    () => _server.Start(),
                    TaskCreationOptions.LongRunning
                )
                .ContinueWith(
                    t => logger.Error(t.Exception),
                    TaskContinuationOptions.OnlyOnFaulted
                );

                WaitForBreakSignal();
            }
            catch (Exception ex)
            {
                logger.Error(null, ex);
            }
            finally
            {
                if (_server != null) _server.Dispose();
            }

            logger.Info("Terminating program");
        }

        static void ShowTitle()
        {
            Console.WriteLine("Configgy: Easy, shared configuration\n");
        }

        static void WaitForBreakSignal()
        {
            var quitEvent = new ManualResetEvent(false);

            Console.CancelKeyPress += (sender, ev) =>
            {
                ev.Cancel = true;
                quitEvent.Set();
            };

            quitEvent.WaitOne();
        }
    }
}
