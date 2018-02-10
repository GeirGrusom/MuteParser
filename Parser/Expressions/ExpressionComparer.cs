using Parser.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Tests
{
    public class ExpressionComparer : IEqualityComparer<Expression>
    {
        public bool Equals(Expression x, Expression y)
        {
            var visitor = new ExpressionEqualityVisitor();
            return visitor.Visit(x, y) == Constant.True;
        }

        public int GetHashCode(Expression obj)
        {
            return obj.ToString().GetHashCode();
        }
    }
}
