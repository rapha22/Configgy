using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Configgy.Server;
using Fclp;
using Newtonsoft.Json;

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

                var options = ParseArguments(args);

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

                logger.Info("Terminating program, press any key to continue...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                logger.Error(null, ex);
            }
            finally
            {
                _server.Dispose();
            }
        }

        static void ShowTitle()
        {
            Console.WriteLine("Configgy: Easy, shared configuration\n");
        }

        static ConfiggyServerOptions ParseArguments(string[] args)
        {
            var options = ReadConfigurationFile(args);

            if (options != null) return options;

            var p = new FluentCommandLineParser<ConfiggyServerOptions>();

            p.Setup(x => x.ConfigurationFilesDirectory)
                .As('d')
                .WithDescription("Path of the directory containing the configuration files (the app will search recursively)")
                .Required();

            p.Setup(x => x.RedisConnectionString)
                .As('r', "redis")
                .WithDescription("Redis connection string")                
                .Required();

            p.Setup(x => x.FilesFilter)
                .As("files-filter")
                .WithDescription("File system filter that will be used to search the files within the base path (default *.json)")                
                .SetDefault("*.json");

            p.Setup(x => x.Prefix)
                .As("prefix")
                .WithDescription("Optional prefix for the configuration IDs on the database");
                

            p.SetupHelp("?", "h", "help")
                .Callback(x =>
                {
                    Console.WriteLine("usage:");
                    Console.WriteLine("\tconfiggy <configuration_file_path>");
                    Console.WriteLine("\tconfiggy -d <base_directory> (-r | redis) <redis_connection_string> [--files-filter <filter_string>] [--prefix <prefix>]");
                    Console.WriteLine(x);
                });

            var result = p.Parse(args);

            if (result.HasErrors)
            {
                Console.WriteLine("Invalid parameters:");
                Console.WriteLine(result.Errors);
                p.HelpOption.ShowHelp(p.Options);
                Environment.Exit(0);
            }

            return p.Object;
        }

        static ConfiggyServerOptions ReadConfigurationFile(string[] args)
        {
            if (args.Length != 1) return null;

            try
            {
                using (var reader = new StreamReader(args[0]))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return new JsonSerializer().Deserialize<ConfiggyServerOptions>(jsonReader);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading configuration file: " + ex.Message, ex);
            }
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
