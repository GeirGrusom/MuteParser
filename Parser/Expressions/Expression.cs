using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public abstract class Expression
    {
        public List<object> Trivia { get; }

        public TypeShim Type { get; }

        protected Expression(TypeShim type)
        {
            Trivia = new List<object>();
            Type = type;
        }

        protected Expression() : this(Types.Void)
        {
        }
    }
}
