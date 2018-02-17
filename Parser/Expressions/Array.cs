using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser.Expressions
{
    public sealed class Array : Expression
    {
        public Array(IEnumerable<Expression> expressions, ArrayTypeShim arrayType)
            : base(arrayType)
        {
            this.Expressions = expressions.ToArray();
        }

        public Expression[] Expressions { get; }

        public override string ToString()
        {
            return $"[{string.Join(", ", Expressions.AsEnumerable())}]";
        }
    }
}
