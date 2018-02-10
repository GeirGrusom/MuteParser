using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions
{
    public class Not : Unary
    {
        public Not(Expression operand) : base(operand)
        {
        }

        public override string ToString() => $"!({Operand})";
    }
}
