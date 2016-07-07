﻿using System;
using System.Collections.Generic;
using Configgy.Common;
using Configgy.TestUtilities;
using StackExchange.Redis;
using Xunit;

namespace Configgy.Server.Tests
{
    public class RedisStorageTests : IDisposable
    {
        ConnectionMultiplexer redisHub;
        const string host = "localhost";
        const string prefix = "RedisStorageTests";

        public RedisStorageTests()
        {
            redisHub = ConnectionMultiplexer.Connect(host + ",allowAdmin=true");

            RedisHelper.RemoveKeys(redisHub, new RedisKeyBuilder(prefix).BuildKey("*"));
        }

        public void Dispose()
        {
            redisHub.Dispose();
        }


        [Fact]
        public void WhenUploadingConfigurationSpace()
        {
            var keyBuilder = new RedisKeyBuilder(prefix);
            var redisStorage = new RedisStorage(redisHub, keyBuilder, new RedisStorageMonitor(redisHub, keyBuilder, new StubLogger()));
            var redis = redisHub.GetDatabase();
            var testKey = keyBuilder.BuildKey("test_key");
            var configurationSpace = new Dictionary<string, object>
            {
                { "keyA", 1 },
                { "keyB", 2 },
                { "keyC", 3 }
            };

            redis.StringSet(testKey, "value");

            redisStorage.UploadConfigurationSpace(configurationSpace);            

            //Should have inserted all keys on Redis
            foreach (var key in configurationSpace.Keys)
            {
                var redisKey = keyBuilder.BuildKey(key);
                Assert.True(redis.KeyExists(redisKey));
            }

            Assert.False(redisHub.GetDatabase().KeyExists(testKey)); //Should remove keys that are not in the configuration set
        }
    }
}