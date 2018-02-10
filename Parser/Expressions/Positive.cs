using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions
{
    public sealed class Positive : Unary
    {
        public Positive(Expression operand) : base(operand)
        {
        }

        public override string ToString() => $"+({Operand})";
    }
}
