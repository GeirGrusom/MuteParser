using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions
{
    public sealed class Negate : Unary
    {
        public Negate(Expression operand) : base(operand)
        {
        }

        public override string ToString() => $"-({Operand})";
    }
}
