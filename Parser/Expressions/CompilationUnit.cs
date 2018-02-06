using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public class CompilationUnit : Expression, IScopedExpression
    {
        public string Module { get; }

        public List<Expression> Body { get; }

        public Scope Scope { get; }

        public CompilationUnit(string name, Scope scope, IEnumerable<Expression> body)
        {
            Module = name;
            Scope = scope ?? new Scope();
            Body = body.ToList();
        }

        public override string ToString()
        {
            return $"module {Module}\r\n\r\n{string.Join("\r\n", Body)}";
        }
    }
}
