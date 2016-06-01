namespace Configgy.Server
{
    public class RedisKeyBuilder
    {
        internal const string GlobalPrefix = "configgy";
        internal const string PrefixSeparator = ":";

        private string _prefix;

        public RedisKeyBuilder(string prefix = null)
        {
            _prefix = prefix;
        }

        internal string BuildKey(string partialKey)
        {
            if (!string.IsNullOrEmpty(_prefix))
                return string.Join(PrefixSeparator, GlobalPrefix, _prefix, partialKey);

            return string.Join(PrefixSeparator, GlobalPrefix, partialKey);
        }
    }
}
