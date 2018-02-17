using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions
{
    public sealed class Minus : Unary
    {
        public Minus(Expression operand) : base(operand)
        {
        }

        public override string ToString() => $"-({Operand})";
    }
}
