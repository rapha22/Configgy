using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using Xunit;

namespace Configgy.Server.Tests
{
    public class RedisStorageMonitorTests
    {
        public class RedisStorageMonitorTestsContext : IDisposable
        {
            protected ConnectionMultiplexer redisHub;

            public RedisStorageMonitorTestsContext()
            {
                redisHub = ConnectionMultiplexer.Connect("localhost,allowAdmin=true");
            }

            public virtual void Dispose()
            {
                redisHub.Dispose();
            }
        }

        public class MonitorChangesTest : RedisStorageMonitorTestsContext, IDisposable
        {
            const string initialKey = "test_key";

            static RedisStorageMonitor monitor;
            static Task<bool> containerTask;            

            public void SetupMonitorWithInitialKey(RedisKeyBuilder keyBuilder)
            {
                monitor = new RedisStorageMonitor(
                    redisHub,
                    keyBuilder,
                    AsyncHelper.CreateCompletionTaskAction(out containerTask)
                );

                monitor.Start();

                redisHub.GetDatabase().StringSet(keyBuilder.BuildKey(initialKey), "fist value");
            }

            public void Dispose()
            {
                monitor.Dispose();
            }

            [Fact]
            public void WhenADeletionOccurs_ShouldTriggerHandlerAction()
            {
                var keyBuilder = new RedisKeyBuilder("WhenADeletionOccurs_ShouldTriggerHandlerAction");

                SetupMonitorWithInitialKey(keyBuilder);

                redisHub.GetDatabase().KeyDelete(keyBuilder.BuildKey(initialKey));

                Assert.True(containerTask.Result);
            }

            [Fact]
            public void WhenAlteringAValue_ShouldTriggerHandlerAction()
            {
                var keyBuilder = new RedisKeyBuilder("WhenAlteringAValue_ShouldTriggerHandlerAction");

                SetupMonitorWithInitialKey(keyBuilder);

                redisHub.GetDatabase().StringSet(keyBuilder.BuildKey(initialKey), "new value");

                Assert.True(containerTask.Result);
            }

            [Fact] 
            public void WhenAddingAValue_ShouldTriggerHandlerAction()
            {
                var keyBuilder = new RedisKeyBuilder("WhenAddingAValue_ShouldTriggerHandlerAction");

                var key = keyBuilder.BuildKey("new-value-key");
                redisHub.GetDatabase().StringSet(key, "new value");

                Assert.True(containerTask.Result);
            }
        }

        public class MonitorPulseTests : RedisStorageMonitorTestsContext, IDisposable
        {
            RedisStorageMonitor monitor;
            bool triggered = false;

            public override void Dispose()
            {
                monitor.Dispose();
                base.Dispose();
            }

            [Fact]
            public void WhenPulseExists_ShouldNotTriggerHandlerAction()
            {
                var keyBuilder = new RedisKeyBuilder("has_pulse_test");

                monitor = new RedisStorageMonitor(
                    redisHub,
                    keyBuilder,
                    () => triggered = true,
                    pulseCheckIntervalMs: 100,
                    eventDelayingMs: 200
                );

                monitor.StartPulseChecker();

                Task.Delay(600).Wait();

                Assert.False(triggered);
            }

            [Fact]
            public void WhenPulseDoesNotExists_ShouldTriggerHandlerAction()
            {
                var keyBuilder = new RedisKeyBuilder("has_no_pulse_test");

                monitor = new RedisStorageMonitor(
                    redisHub,
                    keyBuilder,
                    () => triggered = true,
                    pulseCheckIntervalMs: 100,
                    eventDelayingMs: 200
                );

                monitor.StartPulseChecker();

                RedisHelper.RemoveKeys(redisHub, keyBuilder.BuildKey("*"));

                Task.Delay(350).Wait();

                Assert.True(triggered);
            }
        }

        public class PauseMonitoringForTests : RedisStorageMonitorTestsContext, IDisposable
        {
            RedisKeyBuilder keyBuilder;
            RedisStorageMonitor monitor;
            bool triggered = false;

            public override void Dispose()
            {
                monitor.Dispose();
                base.Dispose();
            }

            public PauseMonitoringForTests()
            {
                keyBuilder = new RedisKeyBuilder("PauseMonitoringForTests");

                monitor = new RedisStorageMonitor(
                    redisHub,
                    keyBuilder,
                    () => triggered = true,
                    pulseCheckIntervalMs: 250
                );

                monitor.Start();            
            }

            [Fact]
            public void WhenExecutingAndActionWhilePaused_ShouldNotTriggerTheHandlerAction()
            {
                monitor.PauseMonitoringFor(() =>
                {
                    redisHub.GetDatabase().StringSet(keyBuilder.BuildKey("test1"), "value");
                    redisHub.GetDatabase().StringSet(keyBuilder.BuildKey("test2"), "value");
                    redisHub.GetDatabase().StringSet(keyBuilder.BuildKey("test3"), "value");
                });

                Task.Delay(500).Wait();

                Assert.False(triggered);
            }
        }
    }
}
