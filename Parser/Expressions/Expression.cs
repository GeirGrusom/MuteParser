using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    using SyntaxTrivia;
    public abstract class Expression
    {
        public List<Trivia> Trivia { get; }

        public TypeShim Type { get; }

        protected Expression(TypeShim type)
        {
            Trivia = new List<Trivia>();
            Type = type;
        }

        protected Expression() : this(Types.Void)
        {
        }
    }
}
