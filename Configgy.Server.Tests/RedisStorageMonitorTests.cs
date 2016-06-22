using System;
using System.Threading.Tasks;
using Configgy.Common;
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
                    new StubLogger()
                );

                monitor.DataSetChanged += AsyncHelper.CreateCompletionTaskAction(out containerTask);
                monitor.Start();

                redisHub.GetDatabase().StringSet(keyBuilder.BuildKey(initialKey), "fist value");
            }

            public override void Dispose()
            {
                monitor.Dispose();
                base.Dispose();
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

                SetupMonitorWithInitialKey(keyBuilder);

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
                    new StubLogger(),
                    pulseCheckIntervalMs: 100,
                    eventDelayingMs: 200
                );

                monitor.DataSetChanged += () => triggered = true;
                monitor.Start();

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
                    new StubLogger(),
                    pulseCheckIntervalMs: 100,
                    eventDelayingMs: 200
                );

                monitor.DataSetChanged += () => triggered = true;
                monitor.Start();

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
                    new StubLogger(),
                    pulseCheckIntervalMs: 250
                );

                monitor.DataSetChanged += () => triggered = true;
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
