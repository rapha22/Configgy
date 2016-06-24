using Xunit;

namespace Configgy.Client.Tests
{
    public class CachedConfigurationSetParserTests
    {
        [Fact]
        public void Parse()
        {
            var stubParser = new StubConfigurationSetParser();
            var parser = new CachedConfigurationSetParser(stubParser, new StubServerMonitor());

            parser.Parse("X");
            parser.Parse("X");
            parser.Parse("X");

            Assert.Equal(1, stubParser.AccessCount);
        }

        [Fact]
        public void Parse_Generic()
        {
            var stubParser = new StubConfigurationSetParser();
            var parser = new CachedConfigurationSetParser(stubParser, new StubServerMonitor());

            parser.Parse<string>("X");
            parser.Parse<string>("X");
            parser.Parse<string>("X");

            Assert.Equal(1, stubParser.AccessCount);
        }

        [Fact]
        public void Parse_Generic_WithPath()
        {
            var stubParser = new StubConfigurationSetParser();
            var parser = new CachedConfigurationSetParser(stubParser, new StubServerMonitor());

            parser.Parse<string>("X", "a");
            parser.Parse<string>("X", "a");
            parser.Parse<string>("X", "a");

            Assert.Equal(1, stubParser.AccessCount);
        }

        [Fact]
        public void Parse_Generic_WithPathAndDifferentTypes()
        {
            var stubParser = new StubConfigurationSetParser();
            var parser = new CachedConfigurationSetParser(stubParser, new StubServerMonitor());

            parser.Parse<string>("X", "a");
            parser.Parse<object>("X", "a");
            parser.Parse<string>("X", "a");
            parser.Parse<object>("X", "a");
            parser.Parse<string>("X", "a");
            parser.Parse<object>("X", "a");

            Assert.Equal(2, stubParser.AccessCount);
        }
    }
}
