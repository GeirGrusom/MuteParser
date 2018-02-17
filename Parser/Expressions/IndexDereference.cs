using System.Collections.Generic;
using System.Linq;

namespace Parser.Expressions
{
    public sealed class IndexDereference : Unary
    {
        public IndexDereference(Expression operand, IEnumerable<Expression> indices) : base(operand, (operand.Type as ArrayTypeShim)?.ArrayType ?? new UnresolvedTypeShim("", false))
        {
            this.Indices = indices.ToArray();
        }

        public Expression[] Indices { get; }

        public override string ToString()
        {
            return $"({Operand})[{string.Join(", ", Indices.AsEnumerable())}]";
        }
    }
}
