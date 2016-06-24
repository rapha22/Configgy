using Xunit;

namespace Configgy.Client.Tests
{
    public class JsonConfigurationSetParserTests
    {
        [Fact]
        public void Parse()
        {
            var parser = new JsonConfigurationSetParser();

            var json = @"{""id"":3, ""nested"": {""name"":""test!""} }";

            dynamic obj = parser.Parse(json);

            Assert.Equal(3, (int)obj.id);
            Assert.Equal("test!", (string)obj.nested.name);
        }

        [Fact]
        public void Parse_WithPathOfDepth0()
        {
            var parser = new JsonConfigurationSetParser();

            var json = @"{""id"":3, ""nested"": {""name"":""test!""} }";

            dynamic obj = parser.Parse<dynamic>(json, "");

            Assert.Equal(3, (int)obj.id);
            Assert.Equal("test!", (string)obj.nested.name);
        }

        [Fact]
        public void Parse_Dynamic_WithPathOfDepth1()
        {
            var parser = new JsonConfigurationSetParser();

            var json = @"{""id"":3, ""nested1"": { ""nested2"": {""name"":""test!""} } }";

            dynamic obj = parser.Parse<dynamic>(json, "nested1");

            Assert.Equal("test!", (string)obj.nested2.name);
        }

        [Fact]
        public void Parse_Dynamic_WithPathOfDepth2()
        {
            var parser = new JsonConfigurationSetParser();

            var json = @"{""id"":3, ""nested1"": { ""nested2"": {""name"":""test!""} } }";

            dynamic obj = parser.Parse<dynamic>(json, "nested1.nested2");

            Assert.Equal("test!", (string)obj.name);
        }

        [Fact]
        public void Parse_Dynamic_WithPathOfDepth3()
        {
            var parser = new JsonConfigurationSetParser();

            var json = @"{""id"":3, ""nested1"": { ""nested2"": {""name"":""test!""} } }";

            var value = parser.Parse<string>(json, "nested1.nested2.name");

            Assert.Equal("test!", value);
        }

        [Fact]
        public void Parse_Generic()
        {
            var parser = new JsonConfigurationSetParser();

            var json = @"{""id"":3, ""nested1"": { ""nested2"": {""name"":""test!""} } }";

            var obj = parser.Parse<Root>(json);

            Assert.Equal(3, obj.id);
            Assert.Equal("test!", obj.nested1.nested2.name);
        }

        [Fact]
        public void Parse_Generic_WithPathOfDepth1()
        {
            var parser = new JsonConfigurationSetParser();

            var json = @"{""id"":3, ""nested1"": { ""nested2"": {""name"":""test!""} } }";

            var obj = parser.Parse<Nested1>(json, "nested1");

            Assert.Equal("test!", obj.nested2.name);
        }

        [Fact]
        public void Parse_Generic_WithPathOfDepth2()
        {
            var parser = new JsonConfigurationSetParser();

            var json = @"{""id"":3, ""nested1"": { ""nested2"": {""name"":""test!""} } }";

            var obj = parser.Parse<Nested2>(json, "nested1.nested2");

            Assert.Equal("test!", obj.name);
        }

        [Fact]
        public void Parse_Generic_WithPathOfDepth3()
        {
            var parser = new JsonConfigurationSetParser();

            var json = @"{""id"":3, ""nested1"": { ""nested2"": {""name"":""test!""} } }";

            var obj = parser.Parse<string>(json, "nested1.nested2.name");

            Assert.Equal("test!", obj);
        }

        class Root
        {
            public int id { get; set; }
            public Nested1 nested1 { get; set; }
        }

        class Nested1
        {
            public Nested2 nested2 { get; set; }
        }

        class Nested2
        {
            public string name { get; set; }
        }
    }
}
