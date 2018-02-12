using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions
{
    public sealed class OrElse : Logical
    {
        public OrElse(Expression left, Expression right) : base(left, right)
        {
        }

        public override string ToString() => $"({Left}) || ({Right})";
    }
}
