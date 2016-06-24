namespace Configgy.Client
{
    interface IConfigurationSetParser
    {
        dynamic Parse(string content);
        T Parse<T>(string content, string propertyPath = null);
    }
}
