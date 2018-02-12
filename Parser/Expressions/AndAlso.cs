using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions
{
    public sealed class AndAlso : Logical
    {
        public AndAlso(Expression left, Expression right) : base(left, right)
        {
        }

        public override string ToString() => $"({Left}) && ({Right})";
    }
}
