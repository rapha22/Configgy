using System;
using Fclp;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.IO;
using Configgy.Server;
using System.Threading;

namespace Configgy
{
    class Program
    {
        static ConfiggyServer _configgyServer;

        static void Main(string[] args)
        {
            try
            {
                ShowTitle();

                var options = ParseArguments(args);                

                new Thread(() =>
                {
                    _configgyServer = new ConfiggyServer(options, Log);
                    _configgyServer.Start();
                })
                .Start();

                Console.ReadLine();

                _configgyServer.Dispose();
            }
            catch (Exception ex)
            {
                var previousColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: " + ex.Message);
                Console.ForegroundColor = previousColor;
            }
        }

        static void Log(string message)
        {
            Console.WriteLine("[{0:dd/MM/yyyy HH:mm:ss.fff}] {1}", DateTime.Now, message);
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
    }
}
