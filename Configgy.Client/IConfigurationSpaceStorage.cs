namespace Configgy.Client
{
    public interface IConfigurationSpaceStorage
    {
        object Get(string key);
    }
}
