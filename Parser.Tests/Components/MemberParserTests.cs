using NUnit.Framework;
using Parser.Components;
using Parser.Expressions;

namespace Parser.Tests.Components
{
    [TestFixture]
    public class MemberParserTests
    {
        [Test]
        public void Parse()
        {
            // Arrange
            var parser = new Parser("a.Length");
            var variable = new Variable("a", false, Types.String);
            parser.PushScopeStack();
            parser.CurrentScope.Add(variable);
            var memberParser = new MemberParser(parser);

            // Act
            var result = memberParser.Parse();

            // Assert
            Assert.That(result, Is.Not.Null); 
        }
    }
}
