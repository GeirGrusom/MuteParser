using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public class Assign : Binary
    {
        public Assign(Expression left, Expression right)
            : base(left, right, left.Type)
        {
        }

        public override string ToString()
        {
            return $"{Left} <- {Right}";
        }
    }
}
