using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Configgy.Server.Tests
{
    public class EventDelayerTests
    {
        [Fact]
        public void WhenCallingTriggerMultipleTimes_ShouldOnlyCallActionOnce()
        {
            var eventDelayer = new EventDelayer(500, true);
            int count = 0;

            eventDelayer.Trigger(() => Interlocked.Increment(ref count));
            eventDelayer.Trigger(() => Interlocked.Increment(ref count));
            eventDelayer.Trigger(() => Interlocked.Increment(ref count));
            eventDelayer.Trigger(() => Interlocked.Increment(ref count));
            eventDelayer.Trigger(() => Interlocked.Increment(ref count));

            Task.Delay(600).Wait();

            Assert.Equal(1, count);
        }

        [Fact]
        public void WhenResetOnTriggerIsOn_ShouldDelayTriggeringOnEachCall()
        {
            var eventDelayer = new EventDelayer(500, true);
            bool triggered = false;

            eventDelayer.Trigger(() => triggered = true);

            Task.Delay(400).Wait();

            eventDelayer.Trigger(() => triggered = true);

            Task.Delay(400).Wait();

            Assert.False(triggered);

            Task.Delay(200).Wait();

            Assert.True(triggered);
        }

        [Fact]
        public void WhenResetOnTriggerIsOff_ShouldNotDelayTriggeringOnEachCall()
        {
            var eventDelayer = new EventDelayer(500, false);
            bool triggered = false;

            eventDelayer.Trigger(() => triggered = true);

            Task.Delay(400).Wait();

            eventDelayer.Trigger(() => triggered = true);

            Task.Delay(200).Wait();

            Assert.True(triggered);
        }
    }
}
