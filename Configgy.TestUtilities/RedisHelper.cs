using StackExchange.Redis;

namespace Configgy.TestUtilities
{
    public class RedisHelper
    {
        public static void RemoveKeys(ConnectionMultiplexer redisHub, string keysPattern)
        {
            var server = redisHub.GetServer(redisHub.GetEndPoints()[0]);
            var db = redisHub.GetDatabase();

            foreach (var key in server.Keys(pattern: keysPattern))
            {
                db.KeyDelete(key);
            }
        }
    }
}
