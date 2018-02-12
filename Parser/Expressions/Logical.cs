using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Expressions
{
    public abstract class Logical : Binary
    {
        protected Logical(Expression left, Expression right) : base(left, right, Types.Bool)
        {
        }
    }
}
