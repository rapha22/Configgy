using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Configgy.Client
{
    internal class JsonConfigurationSetParser : IConfigurationSetParser
    {
        public dynamic Parse(string value)
        {
            return JsonConvert.DeserializeObject(value);
        }

        public T Parse<T>(string value, string propertyPath = null)
        {
            var jsonObject = JObject.Parse(value);

            if (propertyPath == null)
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            else
            {
                var path = propertyPath.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                if (path.Length == 0)
                    return JsonConvert.DeserializeObject<T>(value);

                var token = jsonObject[path[0]];

                for (var i = 1; i < path.Length; i++)
                    token = token[path[i]];

                return token.ToObject<T>();
            }
        }
    }
}
