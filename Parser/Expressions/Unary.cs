using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions
{
    public abstract class Unary : Expression
    {
        protected Unary(Expression operand, TypeShim result)
            : base(result)
        {
            Operand = operand;
        }

        protected Unary(Expression operand)
            : base(operand.Type)
        {
            Operand = operand;
        }

        public Expression Operand { get; }
    }
}
