using NUnit.Framework.Constraints;
using Parser.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Tests
{
    public static class ExpressionIs
    {
        public static IConstraint EqualTo(Expression value)
        {
            return new ExpressionEqualConstraint(value);
        }
    }
}
