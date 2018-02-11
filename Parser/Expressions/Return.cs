using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions
{
    public class Return : Unary
    {
        public Return(Expression operand) : base(operand)
        {
        }

        public Return() : base(null)
        {
        }
    }
}
