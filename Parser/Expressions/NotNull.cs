using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions
{
    public class NotNull : Unary
    {
        public NotNull(Expression operand)
            : base(operand, Types.MakeNonNull(operand.Type))
        {
        }

        public override string ToString() => $"!?({Operand})";
    }
}
