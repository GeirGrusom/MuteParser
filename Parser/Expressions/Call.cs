using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Expressions
{
    public class Call : Expression
    {
        public Call(Expression left, TypeShim returnType, params Expression[] arguments)
            : base(returnType)
        {
            Left = left;
            Arguments = arguments;
            ReturnType = returnType;
        }

        public TypeShim ReturnType { get; }
        public Expression Left { get; }
        public Expression[] Arguments { get; }

        public override string ToString()
        {
            return $"{Left}({String.Join(", ", Arguments.AsEnumerable())})";
        }
    }

    public class TupleCall : Expression
    {
        public TupleCall(Expression left, TypeShim returnType, Expression argument)
            : base(returnType)
        {
            Argument = argument;
            ReturnType = returnType;
        }

        public TypeShim ReturnType { get; }
        public Expression Left { get; }
        public Expression Argument { get; }

        public override string ToString()
        {
            return $"{Left}!{Argument}";
        }
    }
}
