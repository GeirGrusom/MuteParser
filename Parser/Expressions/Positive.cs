using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions
{
    public sealed class Plus : Unary
    {
        public Plus(Expression operand) : base(operand)
        {
        }

        public override string ToString() => $"+({Operand})";
    }
}
