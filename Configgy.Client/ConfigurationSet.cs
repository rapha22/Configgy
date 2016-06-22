namespace Configgy.Client
{
    public class ConfigurationSet
    {
        private object _data;

        public ConfigurationSet(object configurationSetData)
        {
            _data = configurationSetData;
        }

        public dynamic Get()
        {
            return _data;
        }
    }
}
