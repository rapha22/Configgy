using System;
using System.Linq;
using Xunit;

namespace Configgy.Server.Tests
{
    public class GenericExceptionHandlerTests
    {
        [Fact]
        public void WhenInstantiatingWithANullLoggerInstance_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new GenericExceptionHandler(null));
        }

        [Fact]
        public void WhenHandlingAnException_ShouldLog()
        {
            var logger = new StubLogger();
            var handler = new GenericExceptionHandler(logger);
            var ex = new Exception();

            handler.InternalHandle(ex);

            System.Threading.Thread.Sleep(50);

            Assert.Equal(ex, logger.Errors.First().Item2);
        }

        [Fact]
        public void WhenHandlingANullException_ShouldIgnore()
        {
            var logger = new StubLogger();
            var handler = new GenericExceptionHandler(logger);

            handler.InternalHandle(null);

            System.Threading.Thread.Sleep(50);

            Assert.Equal(0, logger.Errors.Count());
        }
    }
}
