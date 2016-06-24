namespace Configgy.Client
{
    public class ConfigurationSet
    {
        private string _content;
        private IConfigurationSetParser _parser;

        internal ConfigurationSet(string content, IConfigurationSetParser parser)
        {
            _content = content;
            _parser = parser;
        }

        public dynamic Get()
        {
            return _parser.Parse(_content);
        }

        public T Get<T>()
        {
            return _parser.Parse<T>(_content);
        }
    }
}
