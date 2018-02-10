using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public class CompilationUnit : Expression, IScopedExpression
    {
        public List<Expression> Body { get; }

        public Scope Scope { get; }

        public CompilationUnit(Scope scope, IEnumerable<Expression> body)
        {
            Scope = scope ?? new Scope();
            Body = body.ToList();
        }

        public override string ToString()
        {
            return string.Join("\r\n", Body);
        }
    }
}
