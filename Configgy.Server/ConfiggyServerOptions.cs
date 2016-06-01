namespace Configgy.Server
{
    public class ConfiggyServerOptions
    {
        public string ConfigurationFilesDirectory { get; set; }
        public string RedisConnectionString { get; set; }
        public string FilesFilter { get; set; }
        public string Prefix { get; set; }
    }
}
