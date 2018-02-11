using NUnit.Framework;
using Parser.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Tests.Components
{
    public static class ParserExtensions
    {
        public static void AssertStackIsEmpty(this ParserComponent parserComponent)
        {
            Assert.That(parserComponent.Parser.StackCount, Is.EqualTo(0));
        }
    }
}
